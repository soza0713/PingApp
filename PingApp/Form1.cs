using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;

namespace PingApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListView();
        }

        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.Columns.Add("Adresse IP", 150);
            listView1.Columns.Add("Temps de réponse (ms)", 200);
            listView1.Columns.Add("Statut", 150);
        }

        private void buttonPing_Click(object sender, EventArgs e)
        {
            infoLabel.Text = "";

            var baseIp = textAdresse.Text;  // L'adresse IP de départ
            var numberOfPingsText = textNombrePings.Text;  // Nombre d'adresses à pinger

            if (!TestIp(baseIp))
            {
                MessageBox.Show("Adresse Incorrecte.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(numberOfPingsText, out int numberOfPings) || numberOfPings <= 0)
            {
                MessageBox.Show("Le nombre d'adresses à pinger est invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lancer les pings pour l'adresse de base et les suivantes
            var tasks = new List<Task>();
            for (int i = 0; i < numberOfPings; i++)
            {
                string ipToPing = IncrementIp(baseIp, i);  // Incrémente l'adresse IP
                tasks.Add(Task.Run(() => PingIP(ipToPing)));
            }

            // Attendre que tous les pings soient terminés
            Task.WhenAll(tasks).ContinueWith(t =>
            {
                // Mettre à jour l'interface utilisateur après le ping
                Invoke(new Action(() =>
                {
                    infoLabel.Text = "Pings terminés.";
                }));
            });
        }

        private bool TestIp(string toTest)
        {
            string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            bool isValid = Regex.IsMatch(toTest, pattern);
            return isValid;
        }

        private void PingIP(string ipToTest)
        {
            Ping pingSender = new Ping();

            try
            {
                PingReply reply = pingSender.Send(ipToTest);
                string status = reply.Status == IPStatus.Success ? "Succès" : $"Échec : {reply.Status}";
                string roundtripTime = reply.Status == IPStatus.Success ? reply.RoundtripTime.ToString() : "N/A";

                ListViewItem item = new ListViewItem(ipToTest);
                item.SubItems.Add(roundtripTime);
                item.SubItems.Add(status);
                Invoke(new Action(() => listView1.Items.Add(item)));

                Invoke(new Action(() =>
                {
                    infoLabel.Text = reply.Status == IPStatus.Success
                        ? $"Temps de réponse : {reply.RoundtripTime} ms"
                        : $"Ping échoué : {reply.Status}";
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    infoLabel.Text = $"Erreur : {ex.Message}";
                    ListViewItem item = new ListViewItem(ipToTest);
                    item.SubItems.Add("N/A");
                    item.SubItems.Add($"Erreur : {ex.Message}");
                    listView1.Items.Add(item);
                }));
            }
        }

        private string IncrementIp(string baseIp, int increment)
        {
            var ipParts = baseIp.Split('.');
            var ipBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                ipBytes[i] = byte.Parse(ipParts[i]);
            }

            ipBytes[3] = (byte)(ipBytes[3] + increment);
            return string.Join('.', ipBytes);
        }
    }
}
