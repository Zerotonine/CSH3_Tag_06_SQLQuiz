using System;
using System.Collections.Generic;
using System.Data;
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
using System.Data.SqlClient;

namespace CSH3_Tag_06_SQLQuiz
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connstr = @"";
        List<(string Frage, string Antwort)> Fragen = new List<(string, string)>();
        int index;
        PDFViewWindow window;
        bool pdfVisible = false;
        bool nextQ = false;

        DataTable dtFrage = new DataTable();
        DataTable dtAntwort = new DataTable();


        public MainWindow()
        {
            InitializeComponent();

            fetchAllQuestions();
            nextQuestion();
        }

        public void fetchAllQuestions()
        {
            using(SqlConnection conn = new SqlConnection(connstr))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT Frage, Antwort FROM tblFrageAntwort", conn);
                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    while(dr.Read())
                    {
                        Fragen.Add((Frage: dr["Frage"].ToString(), Antwort: dr["Antwort"].ToString()));
                    }
                    cmd.Dispose();
                    dr.Close();
                }
                catch
                { }
                finally
                {
                    conn.Close();     
                }
            }
        }

        public void nextQuestion()
        {
            if(Fragen.Count == 0)
            {
                MessageBox.Show("Herzlichen Glückwunsch! Du hast gewonnen...oder so");
                fetchAllQuestions();
            }

            SqlDataAdapter da = null ;
            Random rnd = new Random();
            index = rnd.Next(Fragen.Count);
            tbAufgabe.Text = Fragen[index].Frage;
            using(SqlConnection conn = new SqlConnection(connstr))
            {
                try
                {
                    dtFrage.Clear();
                    da = new SqlDataAdapter(Fragen[index].Antwort, conn);
                    da.Fill(dtFrage);
                    dgAufgabe.ItemsSource = dtFrage.DefaultView;
                }
                catch
                { }
                finally
                {
                    conn.Close();
                    da.Dispose();
                }
            }
        }

        public bool validateAnswer()
        {
            bool result = true;
            
            if (dtFrage.Rows.Count != dtAntwort.Rows.Count)
                return false;

            for(int i = 0; i < dtFrage.Rows.Count; i++)
            {
                for(int j = 0; j < dtFrage.Rows[i].ItemArray.Length; j++)
                {
                    if (dtFrage.Rows[i].ItemArray[j].ToString() != dtAntwort.Rows[i].ItemArray[j].ToString())
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        private void btnAusfuehren_Click(object sender, RoutedEventArgs e)
        {
            if (nextQ)
            {
                nextQuestion();
                nextQ = false;
                btnAusfuehren.Content = "Ausführen";
                txtAntwort.Text = "";
                dgLösung.ItemsSource = null;
            }
            else
            {
                SqlDataAdapter da = null;
                string antwort = txtAntwort.Text;

                List<string> BlackList = new List<string>()
                {
                    "INSERT",
                    "UPDATE",
                    "DELETE",
                    "ALTER",
                    "CREATE"
                };

                if(antwort.Split(' ').Any(s=> BlackList.Contains(s.ToUpper())))
                {
                    MessageBox.Show("Nur Select-Abfragen sind zulässig!");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connstr))
                {
                    try
                    {
                        dtAntwort.Clear();
                        da = new SqlDataAdapter(antwort, conn);
                        da.Fill(dtAntwort);
                        dgLösung.ItemsSource = dtAntwort.DefaultView;
                    }
                    catch
                    { }
                    finally
                    {
                        conn.Close();
                        da.Dispose();
                    }
                }

                if (validateAnswer())
                {
                    MessageBox.Show("Sauber Jung!");
                    Fragen.RemoveAt(index);
                    btnAusfuehren.Content = "Nächste Frage";
                    nextQ = true;
                }
                else
                    MessageBox.Show("Samma haste lack jesoffen oder wat?");
            }
        }
        private void btnDiagram_Click(object sender, RoutedEventArgs e)
        {

            if (window == null)
                window = new PDFViewWindow();

            if (!pdfVisible)
            {
                window.Show();
            }
            else
            {
                window.Close();
                window = null;
                GC.Collect();
            }
            pdfVisible = !pdfVisible;
        }
    }
}
