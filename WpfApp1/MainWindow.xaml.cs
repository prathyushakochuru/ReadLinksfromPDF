using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using SautinSoft;
using System.IO;
using HtmlAgilityPack;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void browseimages_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog1 = new System.Windows.Forms.OpenFileDialog();

            //Error Handling
            try
            {
                dialog1.Filter = "Image files (*.pdf)|*.pdf";
                dialog1.Multiselect = false;
                if (dialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string strfilename = dialog1.FileName;
                    txtfilepath.Text = strfilename;
       //             BitmapImage bitmap = new BitmapImage();
       //             bitmap.BeginInit();
       //             bitmap.CreateOptions =
       //BitmapCreateOptions.PreservePixelFormat |
       //BitmapCreateOptions.IgnoreColorProfile;
       //             bitmap.CacheOption =
       //                 BitmapCacheOption.OnLoad;
       //             bitmap.DecodePixelWidth = 200;
       //             bitmap.UriSource = new Uri(strfilename);

       //             bitmap.EndInit();
                    
                }
               // txtfilepath.IsEnabled = true;
                //btnReplace.IsEnabled = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_readpdf_Click(object sender, RoutedEventArgs e)
        {
            // We are using files only for demonstration.
            // The whole conversion process will be done in memory.
            string pdfFile  = txtfilepath.Text.Trim();
            List<string> listlinks = new List<string>();
            byte[] pdfBytes = File.ReadAllBytes(pdfFile);

            var pdfdoc = new Aspose.Pdf.Document(pdfFile);

            pdfdoc.Save(pdfFile.Replace(".pdf", ".html"), Aspose.Pdf.SaveFormat.Html);

            string html_aspose = File.ReadAllText(pdfFile.Replace(".pdf", ".html"));

            PdfFocus f = new PdfFocus();

            f.OpenPdf(pdfFile);
            if (f.PageCount > 0)
            {

                int result = f.ToHtml(pdfFile.Replace(".pdf", ".html"));
                // Let's force the component to store images inside HTML document
                // using base-64 encoding.
                // Thus the component will not use HDD.
                f.HtmlOptions.IncludeImageInHtml = true;

                f.HtmlOptions.InlineCSS = true;

                string html = f.ToHtml();

                
                // Here we have the HTML result as string object.

                // HtmlWeb hw = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html_aspose);
                if(doc.DocumentNode.SelectNodes("//a[@href]") != null)
                {
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        // Get the value of the HREF attribute
                        string hrefValue = link.GetAttributeValue("href", string.Empty);
                        listlinks.Add(hrefValue);
                    }
                    string toDisplay = string.Join(Environment.NewLine, listlinks);
                    System.Windows.Forms.MessageBox.Show(toDisplay);

                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("There are no actions/links in the PDF you selected!");
                }
            }
        }

        private void btn_readpdf_Click_1(object sender, RoutedEventArgs e)
        {
            string filepath = txtfilepath.Text.Trim();

            List<string> listlinks = new List<string>();

            PdfReader reader = new PdfReader(filepath.Trim());

            //List<string> links = new List<string>();
            //   links= reader.GetLinks(1);

            //System.Windows.Forms.MessageBox.Show(links.ToString());

            int numberofpages = reader.NumberOfPages;

            for(int i =1; i <= numberofpages; i++)
            {
                foreach(var items in Getpagelinks(filepath, i))
                {
                    listlinks.Add(items);
                }
            }

            string toDisplay = string.Join(Environment.NewLine, listlinks);

            System.Windows.Forms.MessageBox.Show(toDisplay);
        }

        public List<string> Getpagelinks(string filepath, int page)
        {
            PdfReader reader = new PdfReader(filepath.Trim());
            //string text = string.Empty;
            //for (int page = 1; page <= reader.NumberOfPages; page++)
            //{
            //    text += PdfTextExtractor.GetTextFromPage(reader, page);
            //}
            //reader.Close();
            //System.Windows.Forms.MessageBox.Show(text);

            //List<PdfAnnotation.PdfImportedLink> list = reader.GetLinks(1);

            //Get the current page
            PdfDictionary PageDictionary = reader.GetPageN(page);
            List<string> Ret = new List<string>();

            //Get all of the annotations for the current page
            PdfArray Annots = PageDictionary.GetAsArray(PdfName.ANNOTS);

            //Make sure we have something
            if ((Annots != null))
            {
                

                //Loop through each annotation
                foreach (PdfObject A in Annots.ArrayList)
                {
                    //Convert the itext-specific object as a generic PDF object
                    PdfDictionary AnnotationDictionary = (PdfDictionary)PdfReader.GetPdfObject(A);

                    //Make sure this annotation has a link
                    if (!AnnotationDictionary.Get(PdfName.SUBTYPE).Equals(PdfName.LINK))
                        continue;

                    //Make sure this annotation has an ACTION
                    if (AnnotationDictionary.Get(PdfName.A) == null)
                        continue;
                    //Get the ACTION for the current annotation
                    PdfDictionary AnnotationAction = AnnotationDictionary.GetAsDict(PdfName.A);

                    //Test if it is a URI action (There are tons of other types of actions, some of which might mimic URI, such as JavaScript, but those need to be handled seperately)
                    if (AnnotationAction.Get(PdfName.S).Equals(PdfName.URI))
                    {
                        PdfString Destination = AnnotationAction.GetAsString(PdfName.URI);
                        if (Destination != null)
                            Ret.Add(Destination.ToString());
                    }
                }
                
            }

            return Ret;

        }
    }
}
