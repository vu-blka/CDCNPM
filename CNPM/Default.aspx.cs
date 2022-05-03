using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CNPM
{
    public partial class _Default : Page
    {
        public static List<String> dsBang = new List<string>();
        public static List<String> dsBangDaChon = new List<string>();
        public static List<String> dsCot = new List<string>();
        public static DataTable dt = new DataTable();
        public static DataTable dtFK = new DataTable();
        public static int sohang = 1;
        public static String[,] tam;


        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                this.loadDSBang();
                this.load_ForeignKeys();
                dt.Rows.Add();
                GridView.DataSource = dt;
                GridView.DataBind();
            }
        }

        private void loadDSBang()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["QLVT_DATHANGConnectionString"].ConnectionString;
            SqlCommand cmd = new SqlCommand();

            String query = "SELECT ROW_NUMBER() OVER (ORDER BY NAME) as VALUE, name as TABLE_NAME from SYS.Tables where name != 'sysdiagrams'";

            cmd.CommandText = query;
            cmd.Connection = conn;
            conn.Open();
            SqlDataReader sdr = cmd.ExecuteReader();
            while(sdr.Read())
            {
                ListItem item = new ListItem();
                item.Text = sdr["TABLE_NAME"].ToString();
                item.Value = sdr["VALUE"].ToString();
                dsBang.Add(sdr["TABLE_NAME"].ToString());
                CheckBoxList_Bang.Items.Add(item);
                CheckBoxList_Bang.AutoPostBack = true;
            }
            conn.Close();
        }

        private void loadDSCot(String tableName)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["QLVT_DATHANGConnectionString"].ConnectionString;
                using(SqlCommand cmd = new SqlCommand())
                {
                    string query = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + tableName + "' and COLUMN_NAME not like 'rowguid%'";

                    cmd.CommandText = query;
                    cmd.Connection = conn;
                    conn.Open();
                    using(SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while(sdr.Read())
                        {
                            ListItem item = new ListItem();
                            item.Text = sdr["COLUMN_NAME"].ToString();
                            item.Value = tableName.ToString();
                            dsCot.Add(tableName + "." + item.Text);
                        }
                    }
                    conn.Close();
                }
            }
        }

        private void load_ForeignKeys()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["QLVT_DATHANGConnectionString"].ConnectionString;
            conn.Open();
            //string cmd = "";
            string cmd = "select  parent_object = (select name from sys.tables where object_id = x.parent_object_id) , " +
            "parent_column = (select name from sys.columns where object_id = x.parent_object_id and column_id = x.parent_column_id)," +
            "referenced_object = (select name from sys.tables where object_id = x.referenced_object_id)," +
            "referenced_column = (select name from sys.columns where object_id = x.referenced_object_id and column_id = x.referenced_column_id)" +
            "from sys.foreign_key_columns x ";

            SqlDataAdapter da = new SqlDataAdapter(cmd, conn);
            da.Fill(dtFK);
            conn.Close();
        }

        private void load_DropDownList()
        {
            for(int i = 0; i < sohang; i++)
            {
                DropDownList ddl = (DropDownList)GridView.Rows[i].FindControl("DropDownList_Truong");

                ddl.DataSource = dsCot;
                ddl.DataBind();
            }
        }


        // Copy gridview vào một mảng tạm (2?) chiều 
        protected void CopyGridView()
        {
            tam = new String[sohang, 5];
            for(int i = 0; i < sohang; i++)
            {
                DropDownList dsT = (DropDownList)GridView.Rows[i].FindControl("DropDownList_Truong");
                DropDownList dsSX = (DropDownList)GridView.Rows[i].FindControl("DropDownList_SapXep");
                CheckBox hien = (CheckBox)GridView.Rows[i].FindControl("Checked_HienThi");
                TextBox dk = (TextBox)GridView.Rows[i].FindControl("TextBox_DieuKien");
                TextBox hoac = (TextBox)GridView.Rows[i].FindControl("TextBox_Hoac");
                tam[i, 0] = dsT.SelectedValue;
                tam[i, 1] = dsSX.SelectedValue;
                tam[i, 2] = hien.Checked.ToString();
                tam[i, 3] = dk.Text;
                tam[i, 4] = hoac.Text; 

            }
        }

        //Paste từ mảng tạm vào bảng
        protected void PasteGridView()
        {
            for(int i = 1; i < sohang; i++)
            {
                DropDownList dsT = (DropDownList)GridView.Rows[i].FindControl("DropDownList_Truong");
                DropDownList dsSX = (DropDownList)GridView.Rows[i].FindControl("DropDowList_SapXep");
                CheckBox hien = (CheckBox)GridView.Rows[i].FindControl("Checked_HienThi");
                TextBox dk = (TextBox)GridView.Rows[i].FindControl("TextBox_DieuKien");
                TextBox hoac = (TextBox)GridView.Rows[i].FindControl("TextBox_Hoac");

                if(dsT.Items.Contains(new ListItem(tam[i-1, 0])) == true)
                {
                    dsT.SelectedValue = tam[i - 1, 0];
                    dsSX.SelectedValue = tam[i - 1, 1];
                    if (tam[i - 1, 2].Equals("True")) hien.Checked = true;
                    else hien.Checked = false;
                    dk.Text = tam[i - 1, 3];
                    hoac.Text = tam[i - 1, 4];
                }
            }
        }

        protected void CheckBoxList_Bang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dsCot.Count != 0)
                CopyGridView();

            dsCot.Clear();
            dsBangDaChon.Clear();
            dsBangDaChon.Add("NON_SELECTED");
            dsCot.Add("NON_SELECTED");

            foreach(ListItem item in CheckBoxList_Bang.Items)
            {
                if(item.Selected)
                {
                    dsBangDaChon.Add(item.Text);
                    loadDSCot(item.Text);
                }
            }

            load_DropDownList();
            if (tam == null) return;
            for(int i = 0; i < sohang; i++)
            {
                DropDownList dsT = (DropDownList)GridView.Rows[i].FindControl("DropDownList_Truong");

                if(dsT.Items.Contains(new ListItem(tam[i, 0])) == true)
                {
                    dsT.SelectedValue = tam[i, 0];
                }
                else
                {
                    DropDownList dsSX = (DropDownList)GridView.Rows[i].FindControl("DropDowList_SapXep");
                    CheckBox hien = (CheckBox)GridView.Rows[i].FindControl("Checked_HienThi");
                    TextBox dk = (TextBox)GridView.Rows[i].FindControl("TextBox_DieuKien");
                    TextBox hoac = (TextBox)GridView.Rows[i].FindControl("TextBox_Hoac");
                    dsSX.SelectedIndex = 0;
                    hien.Checked = false;
                    dk.Text = "";
                    hoac.Text = "";
                }
            }
            
        }

        protected void ButtonTruyVan_Click(object sender, EventArgs e)
        {

        }

        protected void ButtonThemHang_Click(object sender, EventArgs e)
        {
            if(dsCot.Count == 0)
            {
                sohang++;
                dt.Rows.Add();
                GridView.DataSource = dt;
                GridView.DataBind();
                return;
            }
            CopyGridView();
            dt.Rows.Add();
            GridView.DataSource = dt;
            GridView.DataBind();
            sohang++;
            load_DropDownList();
            if (tam == null) PasteGridView();
        }

        protected void ButtonXuatBaoCao_Click(object sender, EventArgs e)
        {

        }
    }
}