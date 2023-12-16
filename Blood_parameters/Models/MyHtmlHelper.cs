using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Blood_parameters.Models;

public class MyHtmlHelper
{
    public static string BootstrapTableBody(List<string> headers, List<List<string>> data)
    {
        headers ??= new List<string>();
        data ??= new  List<List<string>>();

        string headerString = "";
        StringBuilder headerStringBuilder =new StringBuilder();
        foreach (string item in headers)
        {
            headerStringBuilder.Append($@"<th data-field=""{item}"" data-filter-control=""input"" data-sortable=""true"">{item}</th>");
        }
        headerString= headerStringBuilder.ToString();

        string dataString = "";
        StringBuilder dataStringBuilder = new StringBuilder();
        foreach (List<string> item in data)
        {
            dataStringBuilder.Append(@"<tr>");
            foreach (string item2 in item)
            {
                dataStringBuilder.Append($@"<td>{item2}</td>");
            }
            dataStringBuilder.Append(@"</th>");
        }
        dataString = dataStringBuilder.ToString();


        string tableString = $@"
<table id=""table""
           data-show-columns=""true""
           data-filter-control=""true""
           data-sticky-header=""true""
           data-toolbar=""#toolbar""
           data-show-search-clear-button=""true"">
        <thead>
            <tr>
                {headerString}
            </tr>
        </thead>
        <tbody>
            {dataString}
        </tbody>
    </table>
    <div id=""toolbar"">
        <button class=""btn btn-secondary"" id=""hide-columns"">
            Приховати всі стовпці
        </button>
        <button class=""btn btn-secondary"" id=""show-columns"">
            Показати всі стовпці
        </button>
    </div>
";
        return tableString;
    }

    public static string BootstrapTableStyles(HttpContext context)
    {
        string Styles = $@"<link href=""https://{context.Request.Host.Value}/lib/bootstrap-table/bootstrap-table.min.css"" rel=""stylesheet"" />
<link href=""https://{context.Request.Host.Value}/lib/bootstrap-table/extensions/sticky-header/bootstrap-table-sticky-header.min.css"" rel=""stylesheet"" />";
        return Styles;
    }

    public static string BootstrapTableScripts(HttpContext context,List<string> headers)
    {
        headers ??= new List<string>();

        string hideString = "";
        StringBuilder hideStringBuilder = new StringBuilder();
        foreach (string item in headers)
        {
            hideStringBuilder.Append($@"$('#table').bootstrapTable('hideColumn', '{item}');");
        }
        hideString = hideStringBuilder.ToString();

        string showString = "";
        StringBuilder showStringBuilder = new StringBuilder();
        foreach (string item in headers)
        {
            showStringBuilder.Append($@"$('#table').bootstrapTable('showColumn', '{item}');");
        }
        showString = showStringBuilder.ToString();


        string Scripts = $@"
    <script src=""https://{context.Request.Host.Value}/lib/bootstrap-table/bootstrap-table.min.js""></script>
    <script src=""https://{context.Request.Host.Value}/lib/bootstrap-table/extensions/filter-control/bootstrap-table-filter-control.min.js""></script>
    <script src=""https://{context.Request.Host.Value}/lib/bootstrap-table/extensions/multiple-sort/bootstrap-table-multiple-sort.min.js""></script>
    <script src=""https://{context.Request.Host.Value}/lib/bootstrap-table/extensions/sticky-header/bootstrap-table-sticky-header.min.js""></script>

    <script>
        $(function ()
        {{
            $('#table').bootstrapTable()
        }})
        $(function ()
        {{
            $('#hide-columns').on('click', function (event)
            {{
                event.preventDefault();
                {hideString}
            }});
            $('#show-columns').on('click', function (event)
            {{
                event.preventDefault();
                {showString}
            }});
        }})
</script>
";
        return Scripts;
    }


    public static string InputFileScripts(string inputId="file", string buttonId="btn", string spanId="span")
    {
        return $@"
        <script>
        events = ['drag', 'dragstart', 'dragend', 'dragover', 'dragenter', 'dragleave', 'drop'];
        for (const event of events) {{
            document.getElementById('{buttonId}').addEventListener(event, (e) => {{
                e.stopPropagation();
                e.preventDefault();
            }})
            document.getElementById('{spanId}').addEventListener(event, (e) => {{
                e.stopPropagation();
                e.preventDefault();
            }})
        }}
        function dropFile(e) {{
            document.getElementById('{inputId}').files = e.dataTransfer.files;
            document.getElementById('{spanId}').innerHTML = e.dataTransfer.files[0].name;
        }}
        document.getElementById('{buttonId}').addEventListener(""drop"", (e) => {{
            dropFile(e);
        }})
        document.getElementById('{buttonId}').addEventListener(""click"", (e) => {{
            document.getElementById('file').click();
        }})
        document.getElementById('{spanId}').addEventListener(""drop"", (e) => {{
            dropFile(e);
        }})
        document.getElementById('{inputId}').addEventListener(""change"", () => {{
            if (document.getElementById('{inputId}').files[0] != undefined) {{
                document.getElementById('{spanId}').innerHTML = document.getElementById('file').files[0].name;
            }}
            else {{
                document.getElementById('{spanId}').innerHTML = """";
            }}
        }})
        </script>
";
    }
}

