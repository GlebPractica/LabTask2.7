using System;
using System.Data;
using System.Windows.Forms;
using Dapper;
using Microsoft.Data.SqlClient;
using Task1.Classes;

namespace Task1.Forms
{
    public partial class Payments : Form
    {
        private Payment? _selectedPayment;
        private Dictionary<int, int>? memberIndexMap;

        private readonly string _connectionString;

        public Payments(string connectionString)
        {
            InitializeComponent();
            this.Load += Payments_Load;
            _connectionString = connectionString;
        }

        private void Payments_Load(object? sender, EventArgs e)
        {
            LoadPayments();
            LoadMembers();
        }

        //Start User Functions

        private void LoadPayments()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();

                var query = "SELECT * FROM PaymentTbl";
                var listPayments = db.Query<Payment>(query).AsList();
                dataGridView1.DataSource = listPayments;
            }
        }

        private void LoadMembers()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();

                var query = "SELECT * FROM MemberTbl";
                var listMembers = db.Query<Member>(query).AsList();

                comboBox1.Items.Clear();
                memberIndexMap = new Dictionary<int, int>();

                for (int i = 0; i < listMembers.Count; i++)
                {
                    Member mem = listMembers[i];
                    comboBox1.Items.Add($"{mem.Id} ФИО: {mem.Name} Оплата: {mem.Amount}");
                    memberIndexMap[mem.Id] = i;
                }
            }
        }

        private void SavePayment(Payment payment)
        {
            if (payment == null)
                throw new Exception("Нечего сохранять");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = @"INSERT INTO PaymentTbl (MemberId, DatePay, Amount) 
                              VALUES (@MemberId, @DatePay, @Amount)";
                db.Execute(query, payment);
            }
        }

        private void RemovePayment(Payment payment)
        {
            if (payment == null)
                throw new Exception("Нечего удалять");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM PaymentTbl WHERE Id = @Id";
                db.Execute(query, new { payment.Id });
            }
        }

        private void RemoveAllPayments()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM PaymentTbl";
                db.Execute(query);
            }
        }

        private int GetIndex(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                return e.RowIndex;

            return -1;
        }

        private Payment? GetPayment(int index)
        {
            if (index >= 0)
                return dataGridView1.Rows[index].DataBoundItem as Payment;

            throw new Exception("Не выбрана оплата");
        }

        private int IdM()
        {
            if (comboBox1.SelectedItem == null)
                return -1;

            try
            {
                string[]? partsStr = comboBox1.SelectedItem.ToString().Split(' ');

                int IDMember = int.Parse(partsStr[0]);

                partsStr = null;

                return IDMember;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return -1;
            }
        }

        private bool IsMemberFind(int idM)
        {
            if (idM < 0)
                throw new Exception("Запись не найдена");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var query = "SELECT COUNT(1) FROM MemberTbl WHERE Id = @Id";
                return db.ExecuteScalar<bool>(query, new { Id = idM });
            }
        }

        private void DefaultValues()
        {
            comboBox1.SelectedIndex = -1;
            dateTimePicker1.Value = DateTime.Now;
            numericUpDown1.Value = numericUpDown1.Minimum;
        }

        //End User Functions

        private void BttnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                int idM = IdM();

                if (IsMemberFind(idM))
                {
                    Payment pay = new Payment()
                    {
                        MemberId = idM,
                        DatePay = dateTimePicker1.Value.Date,
                        Amount = numericUpDown1.Value
                    };

                    SavePayment(pay);

                    MessageBox.Show("Успешно добавлено", "Результат");

                    LoadPayments();
                }
                else
                {
                    MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BttnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedPayment == null)
                    throw new Exception("Оплата не выбрана");

                RemovePayment(_selectedPayment);

                MessageBox.Show("Успешно удалено", "Результат");

                LoadPayments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BttnReset_Click(object sender, EventArgs e)
        {
            DefaultValues();
        }

        private void BttnRemoveAll_Click(object sender, EventArgs e)
        {
            try
            {
                RemoveAllPayments();

                MessageBox.Show("Успешно все удалено", "Результат");

                LoadPayments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int selectedIndex = GetIndex(e);

                _selectedPayment = GetPayment(selectedIndex);

                if (_selectedPayment != null)
                {
                    int MemberId = _selectedPayment.MemberId;

                    if (memberIndexMap.TryGetValue(MemberId, out int idItem))
                    {
                        comboBox1.SelectedIndex = idItem;
                    }
                    else
                    {
                        comboBox1.SelectedIndex = -1;
                    }

                    dateTimePicker1.Value = _selectedPayment.DatePay;
                    numericUpDown1.Value = _selectedPayment.Amount;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
