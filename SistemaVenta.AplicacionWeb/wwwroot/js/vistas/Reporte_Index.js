let tablaData;

$(document).ready(function () {

    // CONFIGURACIÓN DE FECHAS (Ahora sí funcionará porque arreglamos el HTML)
    $("#txtFechaInicio").datepicker({ dateFormat: "dd/mm/yy" });
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" });

    // EVENTO BOTÓN BUSCAR
    $("#btnBuscar").click(function () {

        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
            toastr.warning("", "Debe ingresar fecha inicio y fin");
            return;
        }

        let fechaInicio = $("#txtFechaInicio").val().trim();
        let fechaFin = $("#txtFechaFin").val().trim();
        let nueva_url = `/Reporte/ReporteVenta?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`;

        // LÓGICA: Si la tabla ya existe, solo cambiamos la URL. Si no, la creamos.
        if ($.fn.DataTable.isDataTable('#tbdata')) {
            tablaData.ajax.url(nueva_url).load();
        } else {
            crearTabla(nueva_url);
        }
    });
});

function crearTabla(url) {
    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "fechaRegistro" },
            { "data": "numeroVenta" },
            { "data": "tipoDocumento" },
            { "data": "documentoCliente" },
            { "data": "nombreCliente" },
            { "data": "subTotalVenta" },
            { "data": "impuestoTotalVenta" },
            { "data": "totalVenta" },
            { "data": "producto" },
            { "data": "cantidad" },
            { "data": "precio" },
            { "data": "total" }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Ventas'
            },
            'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        }
    });
}