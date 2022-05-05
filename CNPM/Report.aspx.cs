using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CNPM
{
    public partial class Report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String qr = Session["query"].ToString();
            String tit = Session["title"].ToString();
            XtraReport xtraRP = new XtraReport();
            SqlConnection cnn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();

            String connect = ConfigurationManager.ConnectionStrings["QLVT_DATHANGConnectionString"].ConnectionString;
            cnn.ConnectionString = connect;
            cnn.Open();
            DataSet dt = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(qr, cnn);
            da.Fill(dt);
            xtraRP.DataSource = dt;
            InitBands(xtraRP);
            InitDetailsBaseXrTable(xtraRP, dt, tit);
            ASPxWebDocumentViewer1.OpenReport(xtraRP);
        }

        public void InitBands(XtraReport rep)
        {
            DetailBand detail = new DetailBand();
            PageHeaderBand pageHeader = new PageHeaderBand();
            ReportHeaderBand reportHeader = new ReportHeaderBand();
            ReportFooterBand reportFooter = new ReportFooterBand();

            reportHeader.HeightF = 40;
            detail.HeightF = 20;
            reportFooter.HeightF = 380;
            pageHeader.HeightF = 20;
            rep.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] { reportHeader, detail, pageHeader, reportFooter });
        }

        public void InitDetailsBaseXrTable(XtraReport rep, DataSet ds, String tieuDe)
        {
            ds = ((DataSet)rep.DataSource);
            int colCount = ds.Tables[0].Columns.Count;
            int colWidth = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right)) / colCount;
            rep.Margins = new System.Drawing.Printing.Margins(20, 20, 20, 20);
            XRLabel title = new XRLabel();
            if (tieuDe.Equals(""))
            {
                title.Text = "REPORT DYNAMIC";
            }
            else
            {
                title.Text = tieuDe;
            }
            title.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            title.ForeColor = Color.Orange;
            title.Font = new Font("Times New Roman", 20, FontStyle.Bold, GraphicsUnit.Pixel);
            title.Width = Convert.ToInt32(rep.PageWidth - 50);


            // Tạo table để biểu thị cho Header
            XRTable tableHeader = new XRTable();
            tableHeader.Height = 40;
            tableHeader.BackColor = Color.Gray;
            tableHeader.ForeColor = Color.White;
            tableHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            tableHeader.Font = new Font("Times New Roman", 12, FontStyle.Bold, GraphicsUnit.Pixel);
            tableHeader.Width = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right));
            tableHeader.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100.0F);
            XRTableRow headerRow = new XRTableRow();
            headerRow.Width = tableHeader.Width;
            tableHeader.Rows.Add(headerRow);
            tableHeader.BeginInit();

            // Tạo table để hiển thị dữ liệu
            XRTable tableDetail = new XRTable();
            tableDetail.Height = 20;
            tableDetail.Width = (rep.PageWidth - (rep.Margins.Left + rep.Margins.Right));
            tableDetail.Font = new Font("Times New Roman", 12, FontStyle.Regular, GraphicsUnit.Pixel);
            XRTableRow detailRow = new XRTableRow();
            detailRow.Width = tableDetail.Width;
            tableDetail.Rows.Add(detailRow);
            tableDetail.Padding = new DevExpress.XtraPrinting.PaddingInfo(5, 5, 5, 5, 100.0F);
            tableDetail.BeginInit();

            for (int i = 0; i < colCount; i++)
            {
                XRTableCell headerCell = new XRTableCell();
                headerCell.Text = ds.Tables[0].Columns[i].Caption;
                XRTableCell detailCell = new XRTableCell();
                detailCell.DataBindings.Add("Text", null, ds.Tables[0].Columns[i].Caption);
                if (i == 0)
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                    detailCell.Borders = DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Bottom;
                }
                else
                {
                    headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                    detailCell.Borders = DevExpress.XtraPrinting.BorderSide.All;
                }

                if (i == 0)
                {
                    headerCell.Width = 50;
                    detailCell.Width = 50;
                    detailCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
                }
                else if (i == 1)
                {
                    headerCell.Width = 130;
                    detailCell.Width = 130;
                    headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                }
                else if (i == 2)
                {
                    headerCell.Width = 70;
                    detailCell.Width = 70;
                    headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                }
                else if (i == 4)
                {
                    headerCell.Width = 145;
                    detailCell.Width = 145;
                    headerCell.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                }
                else
                {
                    headerCell.Width = colWidth;
                    detailCell.Width = colWidth;
                }
                detailCell.Borders = DevExpress.XtraPrinting.BorderSide.Bottom | DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right;

                headerRow.Cells.Add(headerCell);
                detailRow.Cells.Add(detailCell);
            }

            tableHeader.EndInit();
            tableDetail.EndInit();

            rep.Bands[BandKind.ReportHeader].Controls.Add(title);
            rep.Bands[BandKind.PageHeader].Controls.Add(tableHeader);
            rep.Bands[BandKind.Detail].Controls.Add(tableDetail);
        }
    }
}