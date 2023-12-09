using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace tagok
{
    public partial class Form1 : Form
    {
        ListBox People = new ListBox();
        Button Read = new Button();
        Button Show = new Button();
        Button Add = new Button();
        Button Delete = new Button();
        TextBox Name = new TextBox();
        TextBox Year = new TextBox();   
        TextBox Address= new TextBox();
        TextBox Country = new TextBox();

        dbHandler handler = new dbHandler();
        public Form1()
        {
            InitializeComponent();
            Start();

        }
        void Start()
        {
            AddControls();
            PlaceControls();
            SizeControls();
            ControlText();
            AddEvents();
            AddChecks();
            FormThings();
        }
        void FormThings() {
            this.Text = "Házi feladat. Készítette: Zsár Dániel - 2023. December";
        }
        void AddControls() { 
            Controls.Add(People);
            Controls.Add(Read);
            Controls.Add(Show);
            Show.Enabled = false;
            Controls.Add(Add);
            Add.Enabled = false;
            Controls.Add(Delete);
            Delete.Enabled = false;

            Controls.Add(Name);
            Controls.Add(Year);
            Controls.Add(Address);
            Controls.Add(Country);
        }
        
        void PlaceControls() {
            People.Location = new Point(10, 30);
            Read.Location = new Point(10, 350);
            Show.Location = new Point(100, 350);
            Add.Location= new Point(540, 300);
            Delete.Location= new Point(630, 300);

            Name.Location = new Point(540, 60);
            Year.Location = new Point(540, 120);
            Address.Location = new Point(540, 180);
            Country.Location = new Point(540, 240);
        }
        void SizeControls() {
            People.Size = new Size(500, 300);
        }
        void ControlText() {
            Read.Text = "Beolvas";
            Show.Text = "Kiír";
            Add.Text = "Hozzáad";
            Delete.Text = "Töröl";
        }
        void AddEvents() {
            Read.Click += (s, e) => {
                handler.ReadDb();
                Show.Enabled = true;
            };
            Show.Click += (s, e) => {
                displayPeople();
            };
            Add.Click += (s, e) => {
                if (handler.InsertPerson(new Tagok(Name.Text, Convert.ToInt32(Year.Text), Convert.ToInt32(Address.Text), Country.Text))) { 
                    displayPeople();
                }
            };
            Delete.Click += (s, e) => {
                if (handler.deletePerson(Tagok.all[People.SelectedIndex].ID)) {
                    handler.ReadDb();
                    displayPeople();
                }
            };

        }
        void AddChecks() {
            People.SelectedIndexChanged += (s, e) => {
                if (People.SelectedIndex >= 0)
                {
                    Delete.Enabled = true;
                }
                else { Delete.Enabled = false; }
            };
            Read.TextChanged += (s, e) => {
                if (inputCheck())
                {
                    Add.Enabled = true;
                }
                else { Add.Enabled = false; }
            };
            Year.TextChanged += (s, e) => {
                if (inputCheck())
                {
                    Add.Enabled = true;
                }
                else { Add.Enabled = false; }
            };
            Address.TextChanged += (s, e) => {
                if (inputCheck())
                {
                    Add.Enabled = true;
                }
                else { Add.Enabled = false; }
            };
            Country.TextChanged += (s, e) => {
                if (inputCheck())
                {
                    Add.Enabled = true;
                }
                else { Add.Enabled = false; }
            };
        }
        bool inputCheck() {
            if (Read.Text.Length > 0 && Year.Text.Length == 4 && Address.Text.Length == 4 && Country.Text.Length > 0 && Country.Text.Length < 4)
            {
                int temp = 0;
                if (int.TryParse(Year.Text, out temp) && int.TryParse(Address.Text, out temp))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        void displayPeople() {
            People.Items.Clear();
            foreach (var item in Tagok.all)
            {
                People.Items.Add(item.ToString());
            }
        }
    }
    public class Tagok {
        public static List<Tagok> all = new List<Tagok>();
        private int Azon;
        private string Nev;
        private int SzulEv;
        private int IrSzam;
        private string Orsz;
        public int ID { get { return Azon; } }
        public string Name { get { return Nev; } }
        public int Year { get { return SzulEv; } }
        public int Address { get { return IrSzam; } }
        public string Country { get { return Orsz; } }
        public static void Error(Tagok error) { 
            all.Remove(error);
        }
        public Tagok(string name, int year, int address, string country) { 
            Nev = name;
            SzulEv = year;
            IrSzam = address;
            Orsz = country;
            Azon = all.Max(x => x.ID);
            Azon++;
            all.Add(this);
        }
        public Tagok(MySqlDataReader data) {
            Azon = data.GetInt32(data.GetOrdinal("Azon"));
            Nev = data.GetString(data.GetOrdinal("Nev"));
            SzulEv = data.GetInt32(data.GetOrdinal("SzulEv"));
            IrSzam = data.GetInt32(data.GetOrdinal("IrSzam"));
            Orsz = data.GetString(data.GetOrdinal("Orsz"));
            all.Add(this);
        }
        public override string ToString()
        {
            return $"{Azon} - {Nev} - {SzulEv} - {IrSzam} - {Orsz}";
        }

    }
    public class dbHandler {
        private string serverAddress;
        private string dbName;
        private string username;
        private string password;
        static private string connectionString;
        MySqlConnection connection;
        public dbHandler() {
            serverAddress = "127.1.1.1";
            dbName = "hazi";
            username = "user";
            password = "pass";
            connectionString = $"Server={serverAddress};Database={dbName};User={username};Password={password};";
            connection = new MySqlConnection(connectionString);
        }

        public void ReadDb() {
            Tagok.all.Clear();
            try{
                connection.Open();
                string query = "SELECT * FROM ugyfel";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    new Tagok(reader);
                }
                reader.Close();
                command.Dispose();
                connection.Close();
            }
            catch (Exception e){
                MessageBox.Show($"Error: {e.Message}");
            }
        }
        public bool InsertPerson(Tagok person) {
            try {
                connection.Open();
                string query = $"INSERT INTO `ugyfel` VALUES ('{person.ID}', '{person.Name}' , {person.Year}, {person.Address}, '{person.Country}'  );";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
                command.Dispose();
                connection.Close();
                return true;
            }
            catch (Exception e) {
                Tagok.Error(person);
                MessageBox.Show($"Error: {e.Message}");
                return false;
            }
        }
        public bool deletePerson(int id)
        {
            try
            {
                connection.Open();
                string query = $"delete from `ugyfel` where azon = {id};";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
                command.Dispose();
                connection.Close();
                //Tagok.all.RemoveAt(id);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}");
                return false;
            }
        }
    }
}
