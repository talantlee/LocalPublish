using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoLocalPublish
{
    public partial class MoveRootForm : Form
    {
        public MoveRootForm()
        {
            InitializeComponent();
        }
        //private static readonly string[] RootExternalDLLs = new string[] { "FluentFTP.dll", "ICSharpCode.SharpZipLib.dll", "Infragistics.Base.v5.2.dll", "Infragistics.Win.UltraWinEditors.v5.2.dll", "itext.html2pdf.dll", "itext.io.dll", "itext.kernel.dll", "itext.layout.dll", "itext.styledxmlparser.dll", "MathNet.Numerics.dll", "Microsoft.AspNetCore.Connections.Abstractions.dll", "Microsoft.Bcl.AsyncInterfaces.dll", "Microsoft.Extensions.Configuration.Abstractions.dll", "Microsoft.Extensions.Configuration.Binder.dll", "Microsoft.Extensions.Configuration.dll", "Microsoft.Extensions.DependencyInjection.Abstractions.dll", "Microsoft.Extensions.DependencyInjection.dll", "Microsoft.Extensions.DependencyModel.dll", "Microsoft.Extensions.Features.dll", "Microsoft.Extensions.FileProviders.Abstractions.dll", "Microsoft.Extensions.Hosting.Abstractions.dll", "Microsoft.Extensions.Logging.dll", "Microsoft.Extensions.ObjectPool.dll", "Microsoft.Extensions.Options.ConfigurationExtensions.dll", "Microsoft.IO.RecyclableMemoryStream.dll", "Microsoft.Web.WebView2.Core.dll", "Microsoft.Web.WebView2.WinForms.dll", "Microsoft.Web.WebView2.Wpf.dll", "Newtonsoft.Json.dll", "NLog.dll", "NLog.Extensions.Logging.dll", "System.Text.Encodings.Web.dll", "System.Security.Cryptography.Pkcs.dll", "BouncyCastle.Crypto.dll", "BouncyCastle.Cryptography.dll", "DocumentFormat.OpenXml.dll", "Enums.NET.dll", "NPOI.Core.dll", "NPOI.OOXML.dll", "NPOI.OpenXml4Net.dll", "NPOI.OpenXmlFormats.dll", "Orleans.Clustering.Redis.dll", "Orleans.Core.Abstractions.dll", "Orleans.Core.dll", "Orleans.Runtime.Abstractions.dll", "Orleans.Runtime.dll", "OrleansProviders.dll", "Pipelines.Sockets.Unofficial.dll", "Quartz.dll", "SixLabors.Fonts.dll", "SixLabors.ImageSharp.dll",
        //    "StackExchange.Redis.dll", "System.Configuration.ConfigurationManager.dll", "System.IO.Pipelines.dll", "System.Security.Cryptography.Pkcs.dll", "System.Security.Cryptography.Xml.dll", "System.Text.Encodings.Web.dll",
        //    "System.Text.Json.dll", "ZedGraph.dll" };
        private static readonly string[] RootExternalDLLs = new string[] { "BarcodeStandard.dll", "BaseFrameControls.dll", "Ben.Demystifier.dll", "BouncyCastle.Crypto.dll", "BouncyCastle.Cryptography.dll",
            "BusinessSysEntity.dll", "BusinessSysFacade.dll", "CommonlyControls.dll", "Core.dll", "DocumentFormat.OpenXml.dll", "Enums.NET.dll", "Esprima.dll", "FastColoredTextBox.dll", "FluentFTP.dll",
            "FrameControl.dll", "ICSharpCode.SharpZipLib.dll", "Infragistics.Base.v5.2.dll", "Infragistics.Win.UltraWinEditors.v5.2.dll", "Infragistics.Win.UltraWinGrid.v5.2.dll", "INienMadePF.StartPage.dll",
            "Interop.Microsoft.Office.Interop.Excel.dll", "itext.html2pdf.dll", "itext.io.dll", "itext.kernel.dll", "itext.layout.dll", "itext.styledxmlparser.dll", "Jint.dll", "libSkiaSharp.dll", "MathNet.Numerics.dll",
            "Microsoft.AspNetCore.Connections.Abstractions.dll", "Microsoft.Bcl.AsyncInterfaces.dll", "Microsoft.Extensions.Caching.Abstractions.dll", "Microsoft.Extensions.Caching.Memory.dll",
            "Microsoft.Extensions.Configuration.Abstractions.dll", "Microsoft.Extensions.Configuration.Binder.dll", "Microsoft.Extensions.Configuration.dll", "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
            "Microsoft.Extensions.DependencyInjection.dll", "Microsoft.Extensions.DependencyModel.dll", "Microsoft.Extensions.Features.dll",
            "Microsoft.Extensions.FileProviders.Abstractions.dll", "Microsoft.Extensions.Hosting.Abstractions.dll", "Microsoft.Extensions.Logging.Abstractions.dll", "Microsoft.Extensions.Logging.dll",
            "Microsoft.Extensions.ObjectPool.dll", "Microsoft.Extensions.Options.ConfigurationExtensions.dll", "Microsoft.Extensions.Options.dll", "Microsoft.Extensions.Primitives.dll",
            "Microsoft.IO.RecyclableMemoryStream.dll", "Microsoft.Web.WebView2.Core.dll", "Microsoft.Web.WebView2.WinForms.dll", "Microsoft.Web.WebView2.Wpf.dll", "Newtonsoft.Json.dll",
            "NLog.dll", "NLog.Extensions.Logging.dll", "NPOI.Core.dll", "NPOI.OOXML.dll",
            "NPOI.OpenXml4Net.dll", "NPOI.OpenXmlFormats.dll", "NPOIExtent.dll", "Orleans.Clustering.AdoNet.dll", "Orleans.Clustering.Redis.dll", "Orleans.Core.Abstractions.dll", "Orleans.Core.dll",
            "Orleans.Runtime.Abstractions.dll", "Orleans.Runtime.dll", "OrleansProviders.dll", "Patagames.Pdf.dll", "Patagames.Pdf.Gdi.dll", "Patagames.Pdf.WinForms.dll",
            "pdfium.dll", "PdfiumViewer.dll", "Pipelines.Sockets.Unofficial.dll", "QrCodeGenerator.dll", "QRCoder.dll", "Quartz.dll", "RestSharp.dll", "RJCP.SerialPortStream.dll",
             "SixLabors.Fonts.dll", "SixLabors.ImageSharp.dll", "SkiaSharp.dll", "Spire.Doc.dll", "StackExchange.Redis.dll", "SunnyUI.Common.dll", "SunnyUI.dll",
            "System.Buffers.dll", "System.Collections.Immutable.dll", "System.ComponentModel.Annotations.dll", "System.Configuration.ConfigurationManager.dll",
            "System.Diagnostics.DiagnosticSource.dll", "System.Drawing.Common.dll", "System.IO.Pipelines.dll", "System.Memory.dll", "System.Numerics.Vectors.dll",
            "System.Reflection.Metadata.dll", "System.Runtime.CompilerServices.Unsafe.dll", "System.Security.Cryptography.Pkcs.dll", "System.Security.Cryptography.ProtectedData.dll",
            "System.Security.Cryptography.Xml.dll", "System.ServiceModel.Duplex.dll", "System.ServiceModel.Federation.dll", "System.ServiceModel.Http.dll", "System.ServiceModel.NetFramingBase.dll",
            "System.ServiceModel.NetTcp.dll", "System.ServiceModel.Primitives.dll", "System.ServiceModel.Security.dll", "System.Speech.dll", "System.Text.Encoding.CodePages.dll", "System.Text.Encodings.Web.dll",
            "System.Text.Json.dll", "System.Threading.Channels.dll", "System.Threading.Tasks.Extensions.dll", "SystemFrameworks.Services.dll", "WebView2Loader.dll", "ZedGraph.dll","Refit.dll" };
        private void button1_Click(object sender, EventArgs e)
        {
            string basedir = textBox1.Text;
         //  string[] RootExternalDLLs = Form1.RootExternalDLLs;
            string[] rootFileList = System.IO.Directory.GetFiles(basedir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            if (RootExternalDLLs.Length > 0)
                foreach (string f in rootFileList)
                {
                    if (RootExternalDLLs.Contains(Path.GetFileName(f)))
                    {
                        if (File.Exists(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f))))
                        {
                            try
                            {
                               
                                    File.Delete(f);
                                this.richTextBox1.AppendText($"檔案 {f} 已经移除。\n");
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show("無法移動RootExternalDLLs檔案，請確認目錄是否有權限。" + ex.Message);
                            }
                            this.richTextBox1.AppendText($"檔案 {Path.GetFileName(f)} 已存在於 RootExternalDLLs 目錄，跳過移動。\n");
                            continue;
                        }
                         try
                        {
                            if (!Directory.Exists(Path.Combine(basedir, "RootExternalDLLs")))
                            {
                                Directory.CreateDirectory(Path.Combine(basedir, "RootExternalDLLs"));
                            }
                            File.Move(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                            if (File.Exists(f))
                                File.Delete(f);
                            this.richTextBox1.AppendText($"檔案 {Path.GetFileName(f)} 移動成功。\n");
                        }
                        catch
                        {
                            if (File.Exists(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f))))
                                if (RemoveReadOnly(Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f))))
                                {
                                    try
                                    {

                                        File.Move(f, Path.Combine(basedir, "RootExternalDLLs", Path.GetFileName(f)));
                                    }
                                    catch
                                    {
                                        MessageBox.Show("無法移動RootExternalDLLs檔案，請確認目錄是否有權限。");
                                        return;
                                    }
                                }
                        }

                    }
                }
        }

        private bool RemoveReadOnly(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
