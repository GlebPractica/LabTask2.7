using Microsoft.EntityFrameworkCore;
using System;
using System.Windows.Forms;
using Dapper;
using Task1.Classes;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Task1.Forms
{
    public partial class Clients : Form
    {
        private Member? _selectedMember;
        private readonly string connectionString;

        public Clients(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.Load += Clients_Load;
        }

        private void Clients_Load(object? sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            LoadMemberInfo();
        }

        //Start User Functions

        private void LoadMemberInfo()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                db.Open();

                var query = "SELECT * FROM MemberTbl";
                var members = db.Query<Member>(query).AsList();
                dataGridView1.DataSource = members;
            }
        }

        private void SaveMemberTbl(Member newMember)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    var query = @"INSERT INTO MemberTbl (Name, Phone, DateBirthDay, Gen, Amount, Timing) 
                                  VALUES (@Name, @Phone, @DateBirthDay, @Gen, @Amount, @Timing)";
                    db.Execute(query, newMember);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private Member? GetMember(int index)
        {
            if (index >= 0)
                return dataGridView1.Rows[index].DataBoundItem as Member;

            throw new Exception("Не выбран клиент");
        }

        private int GetIndex(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                return e.RowIndex;

            return -1;
        }

        private void RemoveAllMembers()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    var query = "DELETE FROM MemberTbl";
                    db.Execute(query);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void RemoveMember(Member member)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    var query = "DELETE FROM MemberTbl WHERE Id = @Id";
                    db.Execute(query, new { member.Id });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        //End User Functions

        private void BttnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Не заполнены все обязательные поля", "Ошибка");
                return;
            }

            if (dateTimePicker1.Value.Date.Year >= DateTime.Now.Year - 3)
            {
                MessageBox.Show("Неверно введена дата рождения", "Ошибка");
                return;
            }

            try
            {
                Member newMember = new Member()
                {
                    Name = textBox1.Text,
                    Phone = textBox2.Text,
                    DateBirthDay = dateTimePicker1.Value.Date,
                    Gen = textBox3.Text,
                    Amount = numericUpDown1.Value,
                    Timing = comboBox1.SelectedItem.ToString()
                };

                SaveMemberTbl(newMember);

                MessageBox.Show("Успешно добавлено", "Результат");

                LoadMemberInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void BttnReset_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            dateTimePicker1.Value = DateTime.Now.Date;
            textBox3.Text = "";
            numericUpDown1.Value = numericUpDown1.Minimum;
            comboBox1.SelectedIndex = 0;
        }

        private void BttnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedMember == null)
                    throw new Exception("Клиент не выбран");

                RemoveMember(_selectedMember);

                MessageBox.Show("Успешно удалено", "Результат");

                LoadMemberInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BttnRemoveAll_Click(object sender, EventArgs e)
        {
            try
            {
                RemoveAllMembers();

                MessageBox.Show("Все успешно удалено", "Результат");

                LoadMemberInfo();
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

                _selectedMember = GetMember(selectedIndex);

                if (_selectedMember != null)
                {
                    textBox1.Text = _selectedMember.Name;
                    textBox2.Text = _selectedMember.Phone;
                    dateTimePicker1.Value = _selectedMember.DateBirthDay;
                    textBox3.Text = _selectedMember.Gen;
                    numericUpDown1.Value = _selectedMember.Amount;
                    comboBox1.Text = _selectedMember.Timing;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
