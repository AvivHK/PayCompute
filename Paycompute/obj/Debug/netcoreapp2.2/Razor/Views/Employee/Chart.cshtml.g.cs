#pragma checksum "C:\Users\avivhkn\Desktop\Pay-Compute-master\Paycompute\Views\Employee\Chart.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "57626b6111a1ccdf8a77c35dc4af4e9b7fde71c3"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Employee_Chart), @"mvc.1.0.view", @"/Views/Employee/Chart.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Employee/Chart.cshtml", typeof(AspNetCore.Views_Employee_Chart))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Users\avivhkn\Desktop\Pay-Compute-master\Paycompute\Views\_ViewImports.cshtml"
using Paycompute;

#line default
#line hidden
#line 2 "C:\Users\avivhkn\Desktop\Pay-Compute-master\Paycompute\Views\_ViewImports.cshtml"
using Paycompute.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"57626b6111a1ccdf8a77c35dc4af4e9b7fde71c3", @"/Views/Employee/Chart.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f7b159a1e9feef9039727131d2d4070f602f1724", @"/Views/_ViewImports.cshtml")]
    public class Views_Employee_Chart : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "C:\Users\avivhkn\Desktop\Pay-Compute-master\Paycompute\Views\Employee\Chart.cshtml"
  
    ViewData["Title"] = "Our Employees";

#line default
#line hidden
            BeginContext(46, 20, true);
            WriteLiteral("<!DOCTYPE html>\n<h2>");
            EndContext();
            BeginContext(67, 17, false);
#line 5 "C:\Users\avivhkn\Desktop\Pay-Compute-master\Paycompute\Views\Employee\Chart.cshtml"
Write(ViewData["Title"]);

#line default
#line hidden
            EndContext();
            BeginContext(84, 411, true);
            WriteLiteral(@"</h2>


<meta charset=""utf-8"">

<script src=""//d3plus.org/js/d3.js""></script>
<script src=""//d3plus.org/js/d3plus.js""></script>

<div id=""viz"" style=""width:50%;height:50%;top:20%""></div>

<script>
    var visualization = d3plus.viz()
        .container(""#viz"")
        .data(""/files/test.csv"")
         .type(""tree_map"")
        .id(""Position"")
        .depth(1)
    .size(""Count"")
    .draw()
     
</script>

");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591