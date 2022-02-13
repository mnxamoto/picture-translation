using Google.Cloud.Translation.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Tesseract;

namespace Перевод_картинок
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string pathImage = openFileDialog1.FileName;
                string text = "";

                Page page = OCR(pathImage);
                //text = page.GetText();
                //textBox2.Text = text;
                //textBox1.Text = Translate(text);

                Bitmap bmp = new Bitmap(pathImage);
                Graphics g = Graphics.FromImage(bmp);
                SolidBrush brushWhite = new SolidBrush(Color.White);
                SolidBrush brushBlack = new SolidBrush(Color.Black);
                Font drawFont = new Font("Times New Roman", 16);

                var iterator = page.GetIterator();
                PageIteratorLevel pageIteratorLevel = PageIteratorLevel.Block;

                iterator.Begin();

                List<string> texts = new List<string>();
                List<Rectangle> rectangles = new List<Rectangle>();

                do
                {
                    text = iterator.GetText(pageIteratorLevel);
                    //text = Translate(text);
                    texts.Add(text);

                    iterator.TryGetBoundingBox(pageIteratorLevel, out Rect bounds);

                    Rectangle rectangle = new Rectangle(bounds.X1, bounds.Y1, bounds.Width, bounds.Height);
                    rectangles.Add(rectangle);
                }
                while (iterator.Next(pageIteratorLevel));

                iterator.Dispose();

                texts = Translate(texts);

                for (int i = 0; i < texts.Count; i++)
                {
                    g.FillRectangle(brushWhite, rectangles[i]);
                    g.DrawString(texts[i], drawFont, brushBlack, rectangles[i]);
                }

                pictureBox1.Image = bmp;
            }
        }

        private Page OCR(string pathImage)
        {
            var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default);
            Pix img = Pix.LoadFromFile(pathImage);
            Page recognizedPage = engine.Process(img);
            return recognizedPage;
        }

        public List<string> Translate(List<string> texts)
        {
            string text = "";

            int n = texts.Count;

            foreach (var item in texts)
            {
                text += $"{item}. ";
            }
            //text = text.Replace("\n", "%0A");
            text = text.Replace("\n", " ");
            text = text.Replace("|", "I");

            //text = "hello I'm moto ";

            string toLanguage = "ru"; //English
            string fromLanguage = "en"; //Deutsch
            string https = "https://";
            //string url = $"{https}translate.google.ru/?hl=ru&sl={fromLanguage}&tl={toLanguage}&text={text}&op=translate";
            string url = $"{https}translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(text)}";
            //https://translate.google.ru/?hl=ru&sl=en&tl=ru&text=moto&op=translate
            
            var webClient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            var file = webClient.DownloadString(url);
            try
            {
                //result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                JArray fileJSON = (JArray)JsonConvert.DeserializeObject(file);
                List<string> result = new List<string>();

                for (int i = 0; i < n; i++)
                {
                    result.Add((string)fileJSON[0][i][0]);
                    //result += line[i][0];
                }

                return result;
            }
            catch
            {
                throw;
                //return "Error";
            }
        }

        public String Translate2(string text)
        {
            var client = TranslationClient.Create();
            var response = client.TranslateText(text, LanguageCodes.Russian, LanguageCodes.English);
            return response.TranslatedText;
        }
    }
}
