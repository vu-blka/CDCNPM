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

        public static DataSet dataSet = new DataSet();


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
            string cmd = "select  bang_cha = (select name from sys.tables where object_id = x.parent_object_id) , " +
            "khoa_ngoai = (select name from sys.columns where object_id = x.parent_object_id and column_id = x.parent_column_id)," +
            "bang_con = (select name from sys.tables where object_id = x.referenced_object_id)," +
            "khoa_chinh = (select name from sys.columns where object_id = x.referenced_object_id and column_id = x.referenced_column_id)" +
            "from sys.foreign_key_columns x ";

            SqlDataAdapter da = new SqlDataAdapter(cmd, conn);
            da.Fill(dtFK);
            da.Fill(dataSet);
            conn.Close();
            loadThuFK();
        }

        private void loadThuFK()
        {
            foreach(DataRow row in dataSet.Tables[0].Rows)
            {
                string bc = row["bang_cha"].ToString();
                string kn = row["khoa_ngoai"].ToString();
                string bcon = row["bang_con"].ToString();
                string kc = row["khoa_chinh"].ToString();
                Debug.WriteLine("bc: " + bc + " -- kn: " + kn + " -- bcon: " + bcon + " -- kc: " + kc);
            }
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
            tam = new String[sohang, 6];
            for(int i = 0; i < sohang; i++)
            {
                DropDownList dsT = (DropDownList)GridView.Rows[i].FindControl("DropDownList_Truong");
                DropDownList dsSX = (DropDownList)GridView.Rows[i].FindControl("DropDownList_SapXep");
                CheckBox hien = (CheckBox)GridView.Rows[i].FindControl("Checked_HienThi");
                TextBox dk = (TextBox)GridView.Rows[i].FindControl("TextBox_DieuKien");
                TextBox hoac = (TextBox)GridView.Rows[i].FindControl("TextBox_Hoac");
                TextBox rename = (TextBox)GridView.Rows[i].FindControl("TextBox_Rename");
                tam[i, 0] = dsT.SelectedValue;
                tam[i, 1] = dsSX.SelectedValue;
                tam[i, 2] = hien.Checked.ToString();
                tam[i, 3] = dk.Text;
                tam[i, 4] = hoac.Text;
                tam[i, 5] = rename.Text;
            }
        }

        //Paste từ mảng tạm vào bảng
        protected void PasteGridView()
        {
            for(int i = 1; i < sohang; i++)
            {
                DropDownList dsT = (DropDownList)GridView.Rows[i].FindControl("DropDownList_Truong");
                DropDownList dsSX = (DropDownList)GridView.Rows[i].FindControl("DropDownList_SapXep");
                CheckBox hien = (CheckBox)GridView.Rows[i].FindControl("Checked_HienThi");
                TextBox dk = (TextBox)GridView.Rows[i].FindControl("TextBox_DieuKien");
                TextBox hoac = (TextBox)GridView.Rows[i].FindControl("TextBox_Hoac");
                TextBox rename = (TextBox)GridView.Rows[i].FindControl("TextBox_Rename");

                if (dsT.Items.Contains(new ListItem(tam[i-1, 0])) == true)
                {
                    dsT.SelectedValue = tam[i - 1, 0];
                    dsSX.SelectedValue = tam[i - 1, 1];
                    if (tam[i - 1, 2].Equals("True")) hien.Checked = true;
                    else hien.Checked = false;
                    dk.Text = tam[i - 1, 3];
                    hoac.Text = tam[i - 1, 4];
                    rename.Text = tam[i - 1, 5];
                }
            }
        }

        protected void CheckBoxList_Bang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dsCot.Count != 0)
                CopyGridView();

            dsCot.Clear();
            dsBangDaChon.Clear();
            dsCot.Add("Non_Selected");

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
                    DropDownList dsSX = (DropDownList)GridView.Rows[i].FindControl("DropDownList_SapXep");
                    CheckBox hien = (CheckBox)GridView.Rows[i].FindControl("Checked_HienThi");
                    TextBox dk = (TextBox)GridView.Rows[i].FindControl("TextBox_DieuKien");
                    TextBox hoac = (TextBox)GridView.Rows[i].FindControl("TextBox_Hoac");
                    TextBox rename = (TextBox)GridView.Rows[i].FindControl("TextBox_Rename");
                    dsSX.SelectedIndex = 0;
                    hien.Checked = false;
                    dk.Text = "";
                    hoac.Text = "";
                    rename.Text = "";
                }
            }
            
        }

        protected void ButtonTruyVan_Click(object sender, EventArgs e)
        {
            if (dsCot.Count == 0) return;
            CopyGridView();
            string select = "";
            string from = "";
            string orderby = "";
            string groupby = "";
            string where = "";
            string rename = "";
            Boolean groupByFlag = false;
            int soLanSelect = 0;
            int soDongGroupBy = 0;
            int soDongCSMM = 0;


            Debug.WriteLine(dsBangDaChon.Count);
            if (dsBangDaChon.Count >= 2)
            {
                for (int i = 0; i < dsBangDaChon.Count; i++)
                {
                    for (int j = i+1; j < dsBangDaChon.Count; j++)
                    {
                        for (int m = 0; m < dtFK.Rows.Count; m++)
                        {
                            if ((dsBangDaChon[i].Equals(dtFK.Rows[m]["bang_cha"]) && dsBangDaChon[j].Equals(dtFK.Rows[m]["bang_con"])) 
                                || (dsBangDaChon[i].Equals(dtFK.Rows[m]["bang_con"]) && dsBangDaChon[j].Equals(dtFK.Rows[m]["bang_cha"])))
                            {
                                if (!where.Equals("")) where = where + " AND " + dtFK.Rows[m]["bang_cha"].ToString() + "." + dtFK.Rows[m]["khoa_ngoai"].ToString() + " = " + dtFK.Rows[m]["bang_con"].ToString() + "." + dtFK.Rows[m]["khoa_chinh"].ToString();
                                else where = dtFK.Rows[m]["bang_cha"].ToString() + "." + dtFK.Rows[m]["khoa_ngoai"].ToString() + " = " + dtFK.Rows[m]["bang_con"].ToString() + "." + dtFK.Rows[m]["khoa_chinh"].ToString();
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < sohang; i++)
            {
                if (tam[i, 0].Equals("Non_Selected")) continue;
                if (tam[i, 2].Equals("True"))
                {
                    if (tam[i, 1].Equals("Count") || tam[i, 1].Equals("Sum") || tam[i, 1].Equals("Max") || tam[i, 1].Equals("Min"))
                    {
                        if (!select.Equals("")) select = select + ", " + tam[i, 1] + "(" + tam[i, 0] + ")"; // chỗ này nếu không có group by hoặc là FK thì sao, bắt lỗi ra sao?
                        else select = tam[i, 1] + "(" + tam[i, 0] + ")";
                        groupByFlag = true;
                        soDongCSMM++;
                    }
                    else
                    {
                        if (!select.Equals("")) select = select + ", " + tam[i, 0];
                        else select = tam[i, 0];
                    }
                    

                    if (tam[i, 1].Equals("Group by"))
                    {
                        if (!groupby.Equals("")) groupby = groupby + ", " + tam[i, 0];
                        else groupby = tam[i, 0];
                        soDongGroupBy++;
                    }

                    if(!tam[i, 5].Equals(""))
                    {
                        if(tam[i,5].Contains(" "))
                        {
                            Error.Text = "Tên Rename không được có khoảng trống";
                        }
                        else
                        {
                            select = select + " as " + tam[i, 5];
                            Error.Text = "";
                        }
                    }

                    soLanSelect++;
                }
                if(tam[i,1].Equals("ASC") || tam[i,1].Equals("DESC"))
                {
                    if (!orderby.Equals("")) orderby = orderby + ", " + tam[i, 0] + " " + tam[i, 1];
                    else orderby = tam[i, 0] + " " + tam[i, 1];
                }

                if(!tam[i,4].Equals(""))
                {
                    if (tam[i, 3].Equals(""))
                    {
                        if (!where.Equals("")) where = where + " AND " + tam[i, 0] + tam[i, 4];
                        else where = tam[i, 0] + tam[i, 4];
                    }
                    else
                    {
                        if (!where.Equals("")) where = where + " AND ( " + tam[i, 0] + tam[i, 3] + " OR " + tam[i, 0] + tam[i, 4] + " )";
                        else where = " ( " + tam[i, 0] + tam[i, 3] + " OR " + tam[i, 0] + tam[i, 4] + " )";
                    }
                }
                else if(tam[i,4].Equals("") && !tam[i,3].Equals(""))
                {
                    if (!where.Equals("")) where = where + " AND " + tam[i, 0] + tam[i, 3];
                    else where = tam[i, 0] + " " + tam[i, 3];
                }
            }

            from = string.Join(", ", dsBangDaChon);
            if (select.Equals("")) select = "*";
            string sql = "SELECT " + select + " FROM " + from;
            if (!where.Equals("")) sql = sql + " WHERE " + where;
            if (!groupby.Equals("")) sql = sql + " GROUP BY " + groupby;
            if (!orderby.Equals("")) sql = sql + " ORDER BY " + orderby;

            if(from.Equals(""))
            {
                TextBoxSQL.Text = "";
            }
            else
            {
                TextBoxSQL.Text = sql;
            }

            if (soLanSelect >= 2)
            {
                if(groupByFlag == true)
                {
                    if (groupby.Equals(""))
                    {
                        if (soDongCSMM == soLanSelect) Error.Text = "";
                        else
                        {
                            Error.Text = "Chưa có GROUP BY cho hàm SUM/COUNT/MIN/MAX";
                        }
                    }
                    else if (soDongGroupBy + soDongCSMM < soLanSelect)
                    {
                        Error.Text = "Câu lệnh GROUP BY chưa đúng cú pháp 1";
                    }
                    else
                    {
                        Error.Text = "";
                    }
                }
                else
                {
                    if (soDongGroupBy > 0 && soDongGroupBy < soLanSelect)
                    {
                        Error.Text = "Câu lệnh GROUP BY chưa đúng cú pháp 2";
                    }
                    else Error.Text = "";
                }
            }

            groupByFlag = false;
            soLanSelect = 0;
            soDongGroupBy = 0;
            soDongCSMM = 0;
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
            if (tam != null) PasteGridView();           //?????
        }

        protected void ButtonXuatBaoCao_Click(object sender, EventArgs e)
        {
            //if (TextBoxSQL.Text.Equals("")) return;
            //Session["query"] = TextBoxSQL.Text;
            //Session["title"] = TextBoxTuaDe.Text;
            //SqlConnection cnn = new SqlConnection();
            //SqlCommand cmd = new SqlCommand();
            //try
            //{
            //    String connect = ConfigurationManager.ConnectionStrings["QLVT_DATHANGConnectionString"].ConnectionString;
            //    cnn.ConnectionString = connect;
            //    cnn.Open();
            //    DataSet dt = new DataSet();
            //    SqlDataAdapter da = new SqlDataAdapter();
            //    da.SelectCommand = new SqlCommand(TextBoxSQL.Text, cnn);
            //    da.Fill(dt);
            //    Response.Redirect("Report.aspx");
            //    Server.Execute("Report.aspx");
            //}
            //catch (Exception x)
            //{
            //    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Câu truy vấn không hợp lệ !')", true);
            //}
        }
    }
}