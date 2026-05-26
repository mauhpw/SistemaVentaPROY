let ValorImpuesto = 16;

$(document).ready(function () {

    fetch("/Venta/ListaTipoDocumentoVenta")
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboTipoDocumentoVenta").append(
                        $("<option>").val(item.idTipoDocumentoVenta).text(item.descripcion)
                    )
                })
            }
        })

    fetch("/Negocio/Obtener")
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            if (responseJson.estado) {
                const d = responseJson.objeto;
                $("#inputGroupSubTotal").text(`Sub Total - ${d.simboloMoneda}`)
                $("#inputGroupIGV").text(`IVA(16%) - ${d.simboloMoneda}`)
                $("#inputGroupTotal").text(`Total - ${d.simboloMoneda}`)
            }
        })

    fetch("/Cliente/Lista")
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            if (responseJson.data && responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#tbClientesBusqueda tbody").append(
                        $("<tr>").append(
                            $("<td>").text(item.rfc ?? ""),
                            $("<td>").text(item.nombre),
                            $("<td>").append(
                                $("<button>").addClass("btn btn-info btn-sm btn-seleccionar-cliente")
                                    .text("Seleccionar")
                                    .data("rfc", item.rfc ?? "")
                                    .data("nombre", item.nombre)
                            )
                        )
                    )
                })
            }
        })

    $("#btnPublicoGeneral").click(function () {
        $("#txtDocumentoCliente").val("0000")
        $("#txtNombreCliente").val("Público en General")
    })

    $("#cboBuscarProducto").select2({
        ajax: {
            url: "/Venta/ObtenerProductos",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return { busqueda: params.term };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => ({
                        id: item.idProducto,
                        text: item.descripcion,
                        marca: item.marca,
                        categoria: item.nombreCategoria,
                        urlImagen: item.urlImagen,
                        precio: parseFloat(item.precio),
                        descuento: parseFloat(item.descuento ?? 0)
                    }))
                };
            }
        },
        language: "es",
        placeholder: 'Buscar Producto...',
        minimumInputLength: 1,
        templateResult: formatoResultados
    });
})

function formatoResultados(data) {
    if (data.loading) return data.text;
    var contenedor = $(
        `<table width="100%">
        <tr>
            <td style="width:60px">
                <img style="height:60px;width:60px;margin-right:10px" src="${data.urlImagen}"/>
            </td>
            <td>
                <p style="font-weight: bolder;margin:2px">${data.marca}</p>
                <p style="margin:2px">${data.text}</p>
            </td>
        </tr>
        </table>`
    );
    return contenedor;
}

$(document).on("select2:open", function () {
    document.querySelector(".select2-search__field").focus();
})

$(document).on("click", ".btn-seleccionar-cliente", function () {
    $("#txtDocumentoCliente").val($(this).data("rfc"))
    $("#txtNombreCliente").val($(this).data("nombre"))
    $("#modalClientes").modal("hide")
})

let ProductosParaVenta = [];

$("#cboBuscarProducto").on("select2:select", function (e) {
    const data = e.params.data;

    let producto_encontrado = ProductosParaVenta.filter(p => p.idProducto == data.id)
    if (producto_encontrado.length > 0) {
        $("#cboBuscarProducto").val("").trigger("change")
        toastr.warning("", "El producto ya fue agregado")
        return false
    }

    swal({
        title: data.marca,
        text: data.text,
        imageUrl: data.urlImagen,
        type: "input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceholder: "Ingrese Cantidad"
    },
        function (valor) {
            if (valor === false) return false;
            if (valor === "") {
                toastr.warning("", "Necesita ingresar la cantidad")
                return false;
            }
            if (isNaN(parseInt(valor))) {
                toastr.warning("", "Debe ingresar un valor numérico")
                return false;
            }

            const cantidad = parseInt(valor);
            const precio = data.precio;
            const descuentoPct = data.descuento;
            const descuentoMonto = (precio * cantidad * descuentoPct) / 100;
            const subtotal = (precio * cantidad) - descuentoMonto;

            let producto = {
                idProducto: data.id,
                marcaProducto: data.marca,
                descripcionProducto: data.text,
                categoriaProducto: data.categoria,
                cantidad: cantidad,
                precio: precio.toString(),
                descuentoPct: descuentoPct,
                descuentoMonto: descuentoMonto,
                total: subtotal.toString()
            }

            ProductosParaVenta.push(producto)
            mostrarProducto_Precios();
            $("#cboBuscarProducto").val("").trigger("change")
            swal.close()
        }
    )
})

function mostrarProducto_Precios() {
    let totalGeneral = 0;
    let porcentaje = ValorImpuesto / 100;

    $("#tbProducto tbody").html("")

    ProductosParaVenta.forEach((item) => {
        totalGeneral += parseFloat(item.total)

        $("#tbProducto tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text("$" + parseFloat(item.precio).toFixed(2)),
                $("<td>").text("$" + item.descuentoMonto.toFixed(2) + ` (${item.descuentoPct}%)`),
                $("<td>").text("$" + parseFloat(item.total).toFixed(2)),
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-eliminar btn-sm")
                        .append($("<i>").addClass("fas fa-trash-alt"))
                        .data("idProducto", item.idProducto)
                )
            )
        )
    })

    const subtotal = totalGeneral / (1 + porcentaje);
    const igv = totalGeneral - subtotal;

    $("#txtSubTotal").val(subtotal.toFixed(2))
    $("#txtIGV").val(igv.toFixed(2))
    $("#txtTotal").val(totalGeneral.toFixed(2))
}

$(document).on("click", "button.btn-eliminar", function () {
    const _idproducto = $(this).data("idProducto")
    ProductosParaVenta = ProductosParaVenta.filter(p => p.idProducto != _idproducto);
    mostrarProducto_Precios();
})

$("#btnTerminarVenta").click(function () {
    if (ProductosParaVenta.length < 1) {
        toastr.warning("", "Debe ingresar productos")
        return;
    }

    if (!$("#txtNombreCliente").val()) {
        toastr.warning("", "Debe seleccionar un cliente o elegir Público en General")
        return;
    }

    const venta = {
        idTipoDocumentoVenta: $("#cboTipoDocumentoVenta").val(),
        documentoCliente: $("#txtDocumentoCliente").val(),
        nombreCliente: $("#txtNombreCliente").val(),
        subTotal: $("#txtSubTotal").val(),
        impuestoTotal: $("#txtIGV").val(),
        total: $("#txtTotal").val(),
        DetalleVenta: ProductosParaVenta
    }

    $("#btnTerminarVenta").LoadingOverlay("show");

    fetch("/Venta/RegistrarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(venta)
    })
        .then(response => {
            $("#btnTerminarVenta").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                ProductosParaVenta = [];
                mostrarProducto_Precios();
                $("#txtDocumentoCliente").val("")
                $("#txtNombreCliente").val("")
                $("#cboTipoDocumentoVenta").val($("#cboTipoDocumentoVenta option:first").val())
                swal("Registrado!", `Numero Venta: ${responseJson.objeto.numeroVenta}`, "success")
            } else {
                swal("Lo sentimos!", "No se pudo registrar la venta", "error")
            }
        })
})