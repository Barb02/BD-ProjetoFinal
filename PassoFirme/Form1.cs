﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace PassoFirme
{
    public partial class Form1 : Form
    {
        private SqlConnection cn;
        private int currentProduto;
        private int currentFuncionario;
        private int currentFornecedor;
        private int currentRevendedor;
        private int currentSeccao;
        private int seccaoFilter = 5;
        //private bool adding;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Passo Firme";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cn = getSGBDConnection();
            loadProdutos(sender, e);
            loadFuncionarios(sender, e);
            loadFornecedor(sender, e);
            loadRevendedor(sender, e);
            loadSeccao(sender, e);
        }

//SQL connection stuff
        private SqlConnection getSGBDConnection()
        {
            return new SqlConnection("data source=tcp:mednat.ieeta.pt\\SQLSERVER,8101; Initial Catalog = p5g5; uid = p5g5; password = bd_2023");
        }

        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
        }
//End of SQL connection stuff
        
        
//Produto Stuff
        private void loadProdutos(object sender, EventArgs e) {
            if (!verifySGBDConnection())
                return;

            SqlCommand cmd = new SqlCommand("getTiposProduto;", cn);
            listBox_produtos.Items.Clear();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Produto P = new Produto();
                        P.Categoria = reader["categoria"].ToString();
                        P.CustoFabrico = reader["custo_fabrico"].ToString();
                        P.PrecoVenda = reader["preco_venda"].ToString();
                        P.NumProdutos = reader["numProdutos"].ToString();
                        P.NumEncomendas = reader["numEncomendas"].ToString();
                        listBox_produtos.Items.Add(P);
                    }
                }
            }

            cn.Close();
            currentProduto = 0;
            ShowProduto();
        }

        private void ShowProduto() {
            if (listBox_produtos.Items.Count == 0 | currentProduto < 0)
                return;
            Produto prod = new Produto();
            prod = (Produto)listBox_produtos.Items[currentProduto];
            textBox_categoria_produto.Text = prod.Categoria;
            textBox_quantidade.Text = prod.NumProdutos;
            textBox_custo_produto.Text = prod.CustoFabrico;
            textBox_preco_produto.Text = prod.PrecoVenda;
            textBox_numEncomendas_produto.Text = prod.NumEncomendas;
        }

        private void listBox_produtos_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox_produtos.SelectedIndex >= 0) {
                currentProduto = listBox_produtos.SelectedIndex;
                ShowProduto();
            }
        }

        private void button_apagar_produtos_Click(object sender, EventArgs e) {
            if (listBox_produtos.SelectedIndex > -1) {
                try {
                    removeProduto(((Produto)listBox_produtos.SelectedItem).Categoria);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    return;
                }
                listBox_produtos.Items.RemoveAt(listBox_produtos.SelectedIndex);
                MessageBox.Show("Produto removido com sucesso!");
                if (currentProduto == listBox_produtos.Items.Count)
                    currentProduto = listBox_produtos.Items.Count - 1;
                if (currentProduto == -1) {
                    ClearFieldsProduto();
                    MessageBox.Show("Não existem mais produtos!");
                } else
                    ShowProduto();
            }
        }
//TODO OP EDITAR
        private void button_editar_produtos_Click(object sender, EventArgs e) {
            currentProduto = listBox_produtos.SelectedIndex;
            if (currentProduto < 0) {
                MessageBox.Show("Por favor selecione um Produto a editar!");
                return;
            }
            HideButtonsProduto();
            listBox_produtos.Enabled = false;
        }

        private void removeProduto(String codigo_produto) {
            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE Empresa.Produto WHERE codigo_produto=@codigo_produto";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@codigo_produto", codigo_produto);
            cmd.Connection = cn;

            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception("ERRO: Nao foi possivel apagar o produto na BD! \n ERROR MESSAGE: \n" + ex.Message);
            } finally {
                cn.Close();
            }
        }

        public void ClearFieldsProduto() {
            textBox_categoria_produto.Text = "";
            textBox_quantidade.Text = "";
            textBox_custo_produto.Text = "";
            textBox_preco_produto.Text = "";
            textBox_numEncomendas_produto.Text = "";
        }

        public void HideButtonsProduto() {
            button_apagar_produtos.Visible = false;
            button_editar_produtos.Visible = false;
            //TODO criar estes botoes e ações ao clicar neles
            //button_ok_produtos.Visible = true;
            //button_cancelar_produtos.Visible = true;
        }
//End of Produto Stuff


//Funcionario Stuff

        private void loadFuncionarios(object sender, EventArgs e) {
            if (!verifySGBDConnection())
                return;

            listBox_funcionarios.Items.Clear();

            using (SqlCommand cmd = new SqlCommand("getGerentes", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@seccao", SqlDbType.Int).Value = seccaoFilter;

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Funcionario F = new Funcionario();
                        F.Nif = reader["nif"].ToString();
                        F.Nome = reader["nome"].ToString();
                        F.NumCC = reader["numerocc"].ToString();
                        F.Morada = reader["morada"].ToString();
                        F.Id = reader["ID"].ToString();
                        F.Salario = reader["salario"].ToString();
                        F.Seccao = reader["designacao"].ToString();
                        F.SerGerente = "Sim";
                        listBox_funcionarios.Items.Add(F);
                    }
                }
                reader.Close();
            }


            using (SqlCommand cmd2 = new SqlCommand("getOperarios", cn))
            {
                cmd2.CommandType = CommandType.StoredProcedure;
                cmd2.Parameters.Add("@seccao", SqlDbType.Int).Value = seccaoFilter;

                SqlDataReader reader = cmd2.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Funcionario F = new Funcionario();
                        F.Nif = reader["nif"].ToString();
                        F.Nome = reader["nome"].ToString();
                        F.NumCC = reader["numerocc"].ToString();
                        F.Morada = reader["morada"].ToString();
                        F.Id = reader["ID"].ToString();
                        F.Salario = reader["salario"].ToString();
                        F.Seccao = reader["designacao"].ToString();
                        F.SerGerente = "Não";
                        listBox_funcionarios.Items.Add(F);
                    }
                }
            }

            cn.Close();
            currentFuncionario = 0;
            ShowFuncionario();
        }

        private void ShowFuncionario() {
            if (listBox_funcionarios.Items.Count == 0 | currentFuncionario < 0)
                return;
            Funcionario func = new Funcionario();
            func = (Funcionario)listBox_funcionarios.Items[currentFuncionario];
            textBox_nome_funcionario.Text = func.Nome;
            textBox_nif_funcionario.Text = func.Nif;
            textBox_numCC_funcionario.Text = func.NumCC;
            textBox_morada_funcionario.Text = func.Morada;
            textBox_id_funcionario.Text = func.Id;
            textBox_salario_funcionario.Text = func.Salario;
            textBox_seccao_funcionario.Text = func.Seccao;
            textBox_gerente_funcionario.Text = func.SerGerente;
        }

        private void listBox_funcionarios_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox_funcionarios.SelectedIndex >= 0) {
                currentFuncionario = listBox_funcionarios.SelectedIndex;
                ShowFuncionario();
            }
        }


        private void dropdownFunc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dropdownFunc.SelectedIndex >= 0)
            {
                seccaoFilter = dropdownFunc.SelectedIndex;
                loadFuncionarios(sender, e);
            }
        }

        private void button_apagar_funcionario_Click(object sender, EventArgs e) {
            if (listBox_funcionarios.SelectedIndex > -1) {
                try {
                    removeFuncionario(((Funcionario)listBox_funcionarios.SelectedItem).Id);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    return;
                }
                listBox_funcionarios.Items.RemoveAt(listBox_funcionarios.SelectedIndex);
                MessageBox.Show("Funcionario removido com sucesso!");
                if (currentFuncionario == listBox_funcionarios.Items.Count)
                    currentFuncionario = listBox_funcionarios.Items.Count - 1;
                if (currentFuncionario == -1) {
                    ClearFieldsFuncionario();
                    MessageBox.Show("Não existem mais funcionarios!");
                } else
                    ShowFuncionario();
            }
        }
        
//TODO OP EDITAR
        private void button_editar_funcionario_Click(object sender, EventArgs e) {
            currentFuncionario = listBox_funcionarios.SelectedIndex;
            if (currentFuncionario < 0) {
                MessageBox.Show("Por favor selecione um Funcionario a editar!");
                return;
            }
            //HideButtonsFuncionario();
            listBox_funcionarios.Enabled = false;
        }

        private void removeFuncionario(String id_funcionario) {
            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE Empresa.Funcionario WHERE ID=@id_funcionario";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id_funcionario", id_funcionario);
            cmd.Connection = cn;

            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception("ERRO: Nao foi possivel apagar o funcionario na BD! \n ERROR MESSAGE: \n" + ex.Message);
            } finally {
                cn.Close();
            }
        }

        public void ClearFieldsFuncionario() {
            textBox_nome_funcionario.Text = "";
            textBox_nif_funcionario.Text = "";
            textBox_numCC_funcionario.Text = "";
            textBox_morada_funcionario.Text = "";
            textBox_id_funcionario.Text = "";
            textBox_salario_funcionario.Text = "";
            textBox_seccao_funcionario.Text = "";
            textBox_gerente_funcionario.Text = "";
        }

        //public void HideButtonsFuncionario() {
        //    button_apagar_funcionario.Visible = false;
        //    button_editar_funcionario.Visible = false;
        //    //TODO criar estes botoes e ações ao clicar neles
        //    //button_ok_funcionario.Visible = true;
        //    //button_cancelar_funcionario.Visible = true;
        //}
//End of Funcionario Stuff


//Fornecedor Stuff
        private void loadFornecedor(object sender, EventArgs e) {
            if (!verifySGBDConnection())
                return;

            SqlCommand cmd = new SqlCommand("SELECT * FROM Empresa.Fornecedor;", cn);
            SqlDataReader reader = cmd.ExecuteReader();
            listBox_fornecedor.Items.Clear();

            while (reader.Read()) {
                Fornecedor F = new Fornecedor();
                F.Nif = reader["nif"].ToString();
                F.Nome = reader["nome"].ToString();
                F.Morada = reader["morada"].ToString();
                F.Email = reader["email"].ToString();
                listBox_fornecedor.Items.Add(F);
            }
            cn.Close();
            currentFornecedor = 0;
            ShowFornecedor();
        }

        private void ShowFornecedor() {
            if (listBox_fornecedor.Items.Count == 0 | currentFornecedor < 0)
                return;
            Fornecedor forn = new Fornecedor();
            forn = (Fornecedor)listBox_fornecedor.Items[currentFornecedor];

            String numMatPrima;

            cn.Open();

            using (SqlCommand cmd2 = new SqlCommand("getMateriaPrimaFornecida", cn))
            {
                cmd2.CommandType = CommandType.StoredProcedure;

                cmd2.Parameters.Add("@nif", SqlDbType.Int).Value = forn.Nif;
                cmd2.Parameters.Add("@num", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd2.ExecuteNonQuery();

                numMatPrima = Convert.ToString(cmd2.Parameters["@num"].Value);

                if (numMatPrima == "")
                    numMatPrima = "0";
            }

            cn.Close();

            textBox_nome_fornecedor.Text = forn.Nome;
            textBox_nif_fornecedor.Text = forn.Nif;
            textBox_morada_fornecedor.Text = forn.Morada;
            textBox_email_fornecedor.Text = forn.Email;
            quantMatPrima.Text = numMatPrima;
        }

        private void listBox_fornecedor_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox_fornecedor.SelectedIndex >= 0) {
                currentFornecedor = listBox_fornecedor.SelectedIndex;
                ShowFornecedor();
            }
        }

        private void button_apagar_fornecedor_Click(object sender, EventArgs e) {
            if (listBox_fornecedor.SelectedIndex > -1) {
                try {
                    removeFornecedor(((Fornecedor)listBox_fornecedor.SelectedItem).Nif);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    return;
                }
                listBox_fornecedor.Items.RemoveAt(listBox_fornecedor.SelectedIndex);
                MessageBox.Show("Fornecedor removido com sucesso!");
                if (currentFornecedor == listBox_fornecedor.Items.Count)
                    currentFornecedor = listBox_fornecedor.Items.Count - 1;
                if (currentFornecedor == -1) {
                    ClearFieldsFornecedor();
                    MessageBox.Show("Não existem mais fornecedores!");
                } else
                    ShowFornecedor();
            }
        }

//TODO OP EDITAR
        private void button_editar_fornecedor_Click(object sender, EventArgs e) {
            currentFornecedor = listBox_fornecedor.SelectedIndex;
            if (currentFornecedor < 0) {
                MessageBox.Show("Por favor selecione um Fornecedor a editar!");
                return;
            }
            HideButtonsFornecedor();
            listBox_fornecedor.Enabled = false;

        }

        private void removeFornecedor(String nif_fornecedor) {
            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE Empresa.Fornecedor WHERE nif=@nif_fornecedor";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@nif_fornecedor", nif_fornecedor);
            cmd.Connection = cn;

            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception("ERRO: Nao foi possivel apagar o Fornecedor na BD! \n ERROR MESSAGE: \n" + ex.Message);
            } finally {
                cn.Close();
            }
        }

        public void ClearFieldsFornecedor() {
            textBox_nome_fornecedor.Text = "";
            textBox_nif_fornecedor.Text = "";
            textBox_email_fornecedor.Text = "";
            textBox_morada_fornecedor.Text = "";
        }

        public void HideButtonsFornecedor() {
            button_apagar_fornecedor.Visible = false;
            button_editar_fornecedor.Visible = false;
            //TODO criar estes botoes e ações ao clicar neles
            //button_ok_fornecedor.Visible = true;
            //button_cancelar_fornecedor.Visible = true;
        }
//End of Fornecedor Stuff


//Revendedor Stuff
        private void loadRevendedor(object sender, EventArgs e)
        {
            if (!verifySGBDConnection())
                return;

            SqlCommand cmd = new SqlCommand("SELECT * FROM Empresa.Revendedor;", cn);
            SqlDataReader reader = cmd.ExecuteReader();
            listBox_revendedor.Items.Clear();

            while (reader.Read())
            {
                Revendedor R = new Revendedor();
                R.Nif = reader["nif"].ToString();
                R.Nome = reader["nome"].ToString();
                R.Morada = reader["morada"].ToString();
                R.Email = reader["email"].ToString();
                listBox_revendedor.Items.Add(R);
            }

            cn.Close();

            currentRevendedor = 0;
            ShowRevendedor();
        }

        public void ShowRevendedor()
        {
            if (listBox_revendedor.Items.Count == 0 | currentRevendedor < 0)
                return;
            Revendedor rev = new Revendedor();
            rev = (Revendedor)listBox_revendedor.Items[currentRevendedor];

            String numEncomendas;
            String numProdutosEncomendados;

            cn.Open();

            using (SqlCommand cmd2 = new SqlCommand("getEncomendasRevendedor", cn))
            {
                cmd2.CommandType = CommandType.StoredProcedure;

                cmd2.Parameters.Add("@nif", SqlDbType.Int).Value = rev.Nif;

                cmd2.Parameters.Add("@num", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd2.Parameters.Add("@quantidade", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd2.ExecuteNonQuery();

                numEncomendas = Convert.ToString(cmd2.Parameters["@num"].Value);
                numProdutosEncomendados = Convert.ToString(cmd2.Parameters["@quantidade"].Value);
            }

            cn.Close();

            textBox_nome_revendedor.Text = rev.Nome;
            textBox_nif_revendedor.Text = rev.Nif;
            textBox_morada_revendedor.Text = rev.Morada;
            textBox_email_revendedor.Text = rev.Email;
            numEncomendasRevendedor.Text = numEncomendas;
            numProdRevendedor.Text = numProdutosEncomendados;
        }

        private void listBox_revendedor_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listBox_revendedor.SelectedIndex >= 0)
            {
                currentRevendedor = listBox_revendedor.SelectedIndex;
                ShowRevendedor();
            }
        }


//Operações de edição e remoção de revendedores
        private void button_apagar_revendedor_Click(object sender, EventArgs e) {
            if (listBox_revendedor.SelectedIndex > -1) {
                try {
                    removeRevendedor(((Revendedor)listBox_revendedor.SelectedItem).Nif);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    return;
                }
                listBox_revendedor.Items.RemoveAt(listBox_revendedor.SelectedIndex);
                MessageBox.Show("Revendedor removido com sucesso!");
                if (currentRevendedor == listBox_revendedor.Items.Count)
                    currentRevendedor = listBox_revendedor.Items.Count - 1;
                if (currentRevendedor == -1) {
                    ClearFieldsRevendedor();
                    MessageBox.Show("Não existem mais revendedores!");
                } else
                    ShowRevendedor();
            }
        }




//? __________________EDITAR DEVE FUNCIONAR________________________



        private void button_editar_revendedor_Click(object sender, EventArgs e) {
            currentRevendedor = listBox_revendedor.SelectedIndex;
            if (currentRevendedor < 0) {
                MessageBox.Show("Por favor selecione um Revendedor a editar!");
                return;
            }
            HideButtonsRevendedor();
            listBox_revendedor.Enabled = false;
        }

        private void OKRev_Click(object sender, EventArgs e) {
            try {
                SaveRevendedor();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            listBox_revendedor.Enabled = true;
            int idx = listBox_revendedor.FindString(textBox_nif_revendedor.Text);
            listBox_revendedor.SelectedIndex = idx;
            ShowButtonsRevendedor();
        }

        private void cancelarRev_Click(object sender, EventArgs e) {
            listBox_revendedor.Enabled = true;
            if (listBox_revendedor.Items.Count > 0) {
                currentRevendedor = listBox_revendedor.SelectedIndex;
                if (currentRevendedor < 0)
                    currentRevendedor = 0;
                ShowRevendedor();
            } else {
                ClearFieldsRevendedor();
                LockControls();
            }
            ShowButtonsRevendedor();
        }
        
        private void removeRevendedor(String nif_revendedor) {
            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "DELETE Empresa.Revendedor WHERE nif=@nif_revendedor";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@nif_revendedor", nif_revendedor);
            cmd.Connection = cn;

            try {
                cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception("ERRO: Nao foi possivel apagar o Revendedor na BD! \n ERROR MESSAGE: \n" + ex.Message);
            } finally {
                cn.Close();
            }
        }

        private bool SaveRevendedor() {
            Revendedor rev = new Revendedor();
            try {
                rev.Nif = textBox_nif_revendedor.Text;
                rev.Nome = textBox_nome_revendedor.Text;
                rev.Morada = textBox_morada_revendedor.Text;
                rev.Email = textBox_email_revendedor.Text;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return false;
            }
            UpdateRevendedor(rev);
            listBox_revendedor.Items[currentRevendedor] = rev;
            return true;
        }

        private void UpdateRevendedor(Revendedor r) {
            int rows = 0;

            if (!verifySGBDConnection())
                return;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "UPDATE Empresa.Revendedor SET nif=@nif, nome=@nome, morada=@morada, email=@email";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@nif", r.Nif);
            cmd.Parameters.AddWithValue("@nome", r.Nome);
            cmd.Parameters.AddWithValue("@morada", r.Morada);
            cmd.Parameters.AddWithValue("@email", r.Email);
            cmd.Connection = cn;

            try {
                rows = cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                throw new Exception("Failed to update Revendedor in database. \n ERROR MESSAGE: \n" + ex.Message);
            } finally {
                if (rows == 1)
                    MessageBox.Show("Update OK");
                else
                    MessageBox.Show("Update NOT OK");
                cn.Close();
            }
        }

        public void ClearFieldsRevendedor() {
            textBox_nome_revendedor.Text = "";
            textBox_nif_revendedor.Text = "";
            textBox_email_revendedor.Text = "";
            textBox_morada_revendedor.Text = "";
        }

        public void HideButtonsRevendedor() {
            button_apagar_revendedor.Visible = false;
            button_editar_revendedor.Visible = false;
            OKRev.Visible = true;
            cancelarRev.Visible = true;
        }

        public void ShowButtonsRevendedor() {
            button_apagar_revendedor.Visible = true;
            button_editar_revendedor.Visible = true;
            OKRev.Visible = false;
            cancelarRev.Visible = false;
        }

        public void LockControls() {
            textBox_nome_revendedor.ReadOnly = true;
            textBox_nif_revendedor.ReadOnly = true;
            textBox_email_revendedor.ReadOnly = true;
            textBox_morada_revendedor.ReadOnly = true;

        }

        public void UnlockControls() {
            textBox_nome_revendedor.ReadOnly = false;
            textBox_nif_revendedor.ReadOnly = false;
            textBox_email_revendedor.ReadOnly = false;
            textBox_morada_revendedor.ReadOnly = false;
        }
//End of Revendedor Stuff

//Seccao Stuff
        private void loadSeccao(object sender, EventArgs e) {
            if (!verifySGBDConnection())
                return;

            dropdownFunc.Items.Add("All");

            SqlCommand cmd = new SqlCommand("getSeccoes;", cn);
            listBox_seccao.Items.Clear();

            using (SqlDataReader reader = cmd.ExecuteReader()) {
                if (reader.HasRows) {
                    while (reader.Read()) {
                        Seccao S = new Seccao();
                        S.Codigo = reader["codigo"].ToString();
                        S.Designacao = reader["designacao"].ToString();
                        S.NumFunc = reader["numFunc"].ToString();
                        S.NomeGerente = reader["nome"].ToString();
                        listBox_seccao.Items.Add(S);
                        dropdownFunc.Items.Add(S);
                    }
                }
            }
            cn.Close();
            currentSeccao = 0;
            ShowSeccao();
        }

        private void ShowSeccao() {
            if (listBox_seccao.Items.Count == 0 | currentSeccao < 0)
                return;
            Seccao seccao = new Seccao();
            seccao = (Seccao)listBox_seccao.Items[currentSeccao];

            String numEmEspera;
            String numEmProducao;
            String numConcluido;
            double media;

            cn.Open();
            using (SqlCommand cmd2 = new SqlCommand("getNumEstadoBySeccao", cn)) {
                cmd2.CommandType = CommandType.StoredProcedure;

                cmd2.Parameters.Add("@codigo", SqlDbType.Int).Value = seccao.Codigo;

                cmd2.Parameters.Add("@numEmEspera", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd2.Parameters.Add("@numEmProducao", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd2.Parameters.Add("@numConcluido", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd2.ExecuteNonQuery();

                numEmEspera = Convert.ToString(cmd2.Parameters["@numEmEspera"].Value);
                numEmProducao = Convert.ToString(cmd2.Parameters["@numEmProducao"].Value);
                numConcluido = Convert.ToString(cmd2.Parameters["@numConcluido"].Value);
            }

            using (SqlCommand cmd3 = new SqlCommand("getMediaSalarialBySeccao", cn)) {
                cmd3.CommandType = CommandType.StoredProcedure;

                cmd3.Parameters.Add("@codigo", SqlDbType.Int).Value = seccao.Codigo;
                cmd3.Parameters.Add("@media", SqlDbType.Float).Direction = ParameterDirection.Output;

                cmd3.ExecuteNonQuery();

                media = Convert.ToDouble(cmd3.Parameters["@media"].Value);
            }

            listBox_processos.Items.Clear();

            using (SqlCommand cmd4 = new SqlCommand("getProcessos", cn)) {
                cmd4.CommandType = CommandType.StoredProcedure;
                cmd4.Parameters.Add("@seccao", SqlDbType.Int).Value = seccao.Codigo;

                SqlDataReader reader = cmd4.ExecuteReader();
                if (reader.HasRows) {
                    while (reader.Read()){
                        Processo Proc = new Processo();
                        Proc.CodProduto = reader["codigo_produto"].ToString();
                        Proc.IDFuncionario = reader["ID_funcionario"].ToString();
                        Proc.CodMatPrima = reader["codigo_materia_prima"].ToString();
                        Proc.Estado = reader["estado"].ToString();
                        listBox_processos.Items.Add(Proc);
                    }
                }
            }
   
            cn.Close();

            textBox_designacao_seccao.Text = seccao.Designacao;
            textBox_codigo_seccao.Text = seccao.Codigo;
            nomeGerente.Text = seccao.NomeGerente;
            numFunc.Text = seccao.NumFunc;
            mediaSal.Text = media.ToString();
            textBox_emEspera.Text = numEmEspera;
            textBox_emProducao.Text = numEmProducao;
            textBox_concluido.Text = numConcluido;
        }

        private void listBox_seccao_SelectedIndexChanged(object sender, EventArgs e) {
                if (listBox_seccao.SelectedIndex >= 0) {
                    currentSeccao = listBox_seccao.SelectedIndex;
                    ShowSeccao();
                }
        }
//End of Seccao Stuff

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }
    }
}
