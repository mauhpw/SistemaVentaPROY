const VISTA_BUSQUEDA = {
    busquedaFecha: () => {
        $("#txtFechaInicio").val("");
        $("#txtFechaFin").val("");
        $("#txtNumeroVenta").val("");
        $(".busqueda-fecha").show();
        $(".busqueda-venta").hide();
    },
    busquedaVenta: () => {
        $("#txtFechaInicio").val("");
        $("#txtFechaFin").val("");
        $("#txtNumeroVenta").val("");
        $(".busqueda-fecha").hide();
        $(".busqueda-venta").show();
    }
};

$(document).ready(function () {
    VISTA_BUSQUEDA["busquedaFecha"]();
    $.datepicker.setDefaults($.datepicker.regional["es"]);
    $("#txtFechaInicio").datepicker({ dateFormat: "dd/mm/yy" }).datepicker("setDate", new Date());
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" }).datepicker("setDate", new Date());
});

$("#cboBuscarPor").change(function () {
    if ($("#cboBuscarPor").val() == "fecha") {
        VISTA_BUSQUEDA["busquedaFecha"]();
    } else {
        VISTA_BUSQUEDA["busquedaVenta"]();
    }
});

$("#btnBuscar").click(function () {
    if ($("#cboBuscarPor").val() == "fecha") {
        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
            toastr.warning("", "Debe ingresar fecha inicio y fin");
            return;
        }
    } else {
        if ($("#txtNumeroVenta").val().trim() == "") {
            toastr.warning("", "Debe ingresar el numero de venta");
            return;
        }
    }

    let numeroVenta = $("#txtNumeroVenta").val();
    let fechaInicio = $("#txtFechaInicio").val();
    let fechaFin = $("#txtFechaFin").val();

    $(".card-body").find("div.row").LoadingOverlay("show");

    fetch(`/Venta/Historial?numeroVenta=${numeroVenta}&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
        .then(response => {
            $(".card-body").find("div.row").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            $("#tbventa tbody").html("");
            if (responseJson.length > 0) {
                responseJson.forEach((venta) => {
                    const yaFacturado = venta.uuid != null && venta.uuid != "";

                    const btnFactura = $("<button>")
                        .addClass("btn btn-success btn-sm btn-solicitar-factura ms-1")
                        .append($("<i>").addClass("fas fa-file-invoice-dollar"))
                        .append(" Solicitar Factura")
                        .data("venta", venta);

                    if (yaFacturado) {
                        btnFactura.prop("disabled", true)
                            .attr("title", "Esta venta ya fue facturada")
                            .removeClass("btn-success")
                            .addClass("btn-secondary");
                    }

                    $("#tbventa tbody").append($("<tr>").append(
                        $("<td>").text(venta.fechaRegistro),
                        $("<td>").text(venta.numeroVenta),
                        $("<td>").text(venta.tipoDocumentoVenta),
                        $("<td>").text(venta.documentoCliente),
                        $("<td>").text(venta.nombreCliente),
                        $("<td>").text(venta.total),
                        $("<td>").append(
                            $("<button>").addClass("btn btn-info btn-sm").append($("<i>").addClass("fas fa-eye")).data("venta", venta),
                            btnFactura
                        )
                    ));
                });
            }
        });
});

$("#tbventa tbody").on("click", ".btn-info", function () {
    let d = $(this).data("venta");
    $("#txtFechaRegistro").val(d.fechaRegistro);
    $("#txtNumVenta").val(d.numeroVenta);
    $("#txtUsuarioRegistro").val(d.usuario);
    $("#txtTipoDocumento").val(d.tipoDocumentoVenta);
    $("#txtDocumentoCliente").val(d.documentoCliente);
    $("#txtNombreCliente").val(d.nombreCliente);
    $("#txtSubTotal").val(d.subTotal);
    $("#txtIGV").val(d.impuestoTotal);
    $("#txtTotal").val(d.total);

    const yaFacturado = d.uuid != null && d.uuid != "";
    $("#btnSolicitarFactura").prop("disabled", yaFacturado);
    if (yaFacturado) {
        $("#btnSolicitarFactura").removeClass("btn-success").addClass("btn-secondary").attr("title", "Ya facturada");
    } else {
        $("#btnSolicitarFactura").removeClass("btn-secondary").addClass("btn-success").attr("title", "");
    }

    $("#tbProductos tbody").html("");
    d.detalleVenta.forEach((item) => {
        $("#tbProductos tbody").append($("<tr>").append(
            $("<td>").text(item.descripcionProducto),
            $("<td>").text(item.cantidad),
            $("<td>").text(item.precio),
            $("<td>").text(item.total)
        ));
    });

    $("#linkImprimir").attr("href", `/Venta/MostrarPDFVenta?numeroVenta=${d.numeroVenta}`);
    $("#modalData").modal("show");
});

// ✅ CORREGIDO: guarda el numeroVenta en un campo del modal de factura
$("#tbventa tbody").on("click", ".btn-solicitar-factura", function () {
    let d = $(this).data("venta");
    $("#txtNumVentaFactura").val(d.numeroVenta); // campo oculto en modalFactura
    $("#modalFactura").modal("show");
});

$("#btnGenerarFactura").click(function () {
    const numeroVenta = $("#txtNumVentaFactura").val(); // ✅ lee del campo correcto

    const usoCFDI = parseInt($("#cboUsoCFDI").val());
    const regimenFiscal = parseInt($("#cboRegimenFiscalReceptor").val());
    const formaPago = parseInt($("#cboFormaPago").val());
    const metodoPago = parseInt($("#cboMetodoPago").val());
    const tipoComprobante = parseInt($("#cboTipoComprobante").val());

    if (!numeroVenta) {
        toastr.warning("No se encontró el número de venta", "Error");
        return;
    }

    if (isNaN(usoCFDI) || isNaN(regimenFiscal) || isNaN(formaPago) || isNaN(metodoPago) || isNaN(tipoComprobante)) {
        toastr.warning("Por favor seleccione todos los campos del formulario", "Campos incompletos");
        return;
    }

    const datos = {
        NumeroVenta: numeroVenta,
        UsoCFDI: usoCFDI,
        RegimenFiscal: regimenFiscal,
        FormaPago: formaPago,
        MetodoPago: metodoPago,
        TipoComprobante: tipoComprobante
    };

    $("#btnGenerarFactura").LoadingOverlay("show");

    fetch("/Venta/GuardarDatosFactura", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(datos)
    })
        .then(response => {
            $("#btnGenerarFactura").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                $("#modalFactura").modal("hide");
                toastr.success("Datos fiscales guardados correctamente", "Facturación");
                $("#btnBuscar").click();
            } else {
                toastr.error(responseJson.mensaje, "Error");
            }
        })
        .catch(error => {
            $("#btnGenerarFactura").LoadingOverlay("hide");
            toastr.error("Ocurrió un error en la petición", "Error");
        });
});