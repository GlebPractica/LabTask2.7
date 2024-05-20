using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Task1.Classes;

namespace Task1.Forms
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = @"Server=(localdb)\mssqllocaldb;Database=gym;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            BttnWorkClients.Enabled = false;
            BttnWorkPlayments.Enabled = false;

            await CreateDatabaseAsync();

            this.Focus();
        }

        private async Task CreateDatabaseAsync()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var checkDbQuery = "IF DB_ID('gym') IS NULL CREATE DATABASE [gym]";
                    await connection.ExecuteAsync(checkDbQuery);

                    connection.ChangeDatabase("gym");

                    var createMemberTableQuery = @"
                IF OBJECT_ID('MemberTbl', 'U') IS NULL
                CREATE TABLE MemberTbl (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(100) NOT NULL,
                    Phone NVARCHAR(20) NOT NULL,
                    DateBirthDay DATE NOT NULL,
                    Gen NVARCHAR(10) NOT NULL,
                    Amount DECIMAL(18, 2) NOT NULL,
                    Timing NVARCHAR(50) NOT NULL
                )";
                    await connection.ExecuteAsync(createMemberTableQuery);

                    var createPaymentTableQuery = @"
                IF OBJECT_ID('PaymentTbl', 'U') IS NULL
                CREATE TABLE PaymentTbl (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    MemberId INT NOT NULL,
                    DatePay DATE NOT NULL,
                    Amount DECIMAL(18, 2) NOT NULL,
                    FOREIGN KEY (MemberId) REFERENCES MemberTbl(Id)
                )";
                    await connection.ExecuteAsync(createPaymentTableQuery);

                    MessageBox.Show("База данных загружена", "Результат");

                    BttnWorkPlayments.Enabled = true;
                    BttnWorkClients.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                Environment.Exit(1);
            }
        }

        private async Task OpenFormClients()
        {
            BttnWorkClients.Enabled = false;

            using (Clients formClients = new Clients(connectionString))
            {
                formClients.FormClosed += (sender, args) =>
                {
                    if (BttnWorkClients.InvokeRequired)
                        BttnWorkClients.Invoke(new MethodInvoker(() => BttnWorkClients.Enabled = true));
                    else
                        BttnWorkClients.Enabled = false;
                };

                await Task.Run(() =>
                {
                    formClients.ShowDialog();
                    formClients.Dispose();
                });
            }
        }

        private async Task OpenFormPayments()
        {
            BttnWorkPlayments.Enabled = false;

            using (Payments formPayments = new Payments(connectionString))
            {
                formPayments.FormClosed += (sender, args) =>
                {
                    if (BttnWorkPlayments.InvokeRequired)
                        BttnWorkPlayments.Invoke(new MethodInvoker(() => BttnWorkPlayments.Enabled = true));
                    else
                        BttnWorkPlayments.Enabled = false;
                };

                await Task.Run(() =>
                {
                    formPayments.ShowDialog();
                    formPayments.Dispose();
                });
            }
        }

        private async void BttnWorkClients_Click(object sender, EventArgs e)
        {
            await OpenFormClients();
        }

        private async void BttnWorkPlayments_Click(object sender, EventArgs e)
        {
            await OpenFormPayments();
        }
    }
}

