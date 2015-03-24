// NX 9.0.3.4
// Journal created by vlunev on Thu Dec 25 10:03:29 2014 RTZ 2 (зима)
//
using System;
using System.IO;
using NXOpen;
using System.Windows.Forms;
using NXOpen.UF;
using NXOpen.Utilities;


public class NXJournal
{
	public static ListingWindow LW;	//отладочное окно
	public static StreamWriter txtfile;
	
	public static string CreateFirstDialog()
	{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			System.Windows.Forms.TextBox textBox1 = new System.Windows.Forms.TextBox();
			System.Windows.Forms.Button button1 = new System.Windows.Forms.Button();
			System.Windows.Forms.Button button2 = new System.Windows.Forms.Button();
			System.Windows.Forms.Button button3 = new System.Windows.Forms.Button();
			// 
			// textBox1
			// 
			textBox1.Location = new System.Drawing.Point(12, 12);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(394, 20);
			textBox1.TabIndex = 0;
			// 
			// button1
			// 
			button1.Location = new System.Drawing.Point(412, 11);
			button1.Name = "button1";
			button1.Size = new System.Drawing.Size(29, 21);
			button1.TabIndex = 1;
			button1.Text = "...";
			button1.UseVisualStyleBackColor = true;
			button1.Click += delegate(object sender, EventArgs e)
			{
				SaveFileDialog saveDialog = new SaveFileDialog();
				saveDialog.Filter = "KOMPAS file|*.cdw";
				saveDialog.Title = "Сохранение CDW";
				if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) textBox1.Text = saveDialog.FileName;
			};
			// 
			// button2
			// 
			button2.DialogResult = System.Windows.Forms.DialogResult.OK;
			button2.Location = new System.Drawing.Point(131, 38);
			button2.Name = "button2";
			button2.Size = new System.Drawing.Size(75, 23);
			button2.TabIndex = 2;
			button2.Text = "Ok";
			button2.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			button3.Location = new System.Drawing.Point(236, 38);
			button3.Name = "button3";
			button3.Size = new System.Drawing.Size(75, 23);
			button3.TabIndex = 3;
			button3.Text = "Отмена";
			button3.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			Form dForm = new Form();
			dForm.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			dForm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			dForm.ClientSize = new System.Drawing.Size(444, 67);
			dForm.Controls.Add(button3);
			dForm.Controls.Add(button2);
			dForm.Controls.Add(button1);
			dForm.Controls.Add(textBox1);
			dForm.Name = "MainForm";
			dForm.Text = "Путь сохранения CDW";
			
			//NXOpenUI.FormUtilities.ReparentForm(dForm);
			if (dForm.ShowDialog() == System.Windows.Forms.DialogResult.OK) return textBox1.Text;
			return "";
	}
	
	public static int GetKompasObjStyle(DisplayableObject obj)
	{
		if (obj.LineWidth == DisplayableObject.ObjectWidth.Normal) return 1;
		if (obj.LineWidth == DisplayableObject.ObjectWidth.Six) return 2;
		return 1;
	}
	
	public static void PrintSTR(string s)
	{
		//LW.WriteLine(s);
		if (txtfile != null) txtfile.WriteLine(s);
	}
	
	public static double GetX(NXOpen.Drawings.DraftingView view, Point3d p)
	{
		if (view.Matrix.Xx != 0) return p.X;
		if (view.Matrix.Xy != 0) return p.Y;
		if (view.Matrix.Xz != 0) return p.Z;
		return 0;
	}
	
	public static double GetY(NXOpen.Drawings.DraftingView view, Point3d p)
	{
		if (view.Matrix.Yx != 0) return p.X;
		if (view.Matrix.Yy != 0) return p.Y;
		if (view.Matrix.Yz != 0) return p.Z;
		return 0;
	}
	
  public static void Main(string[] args)
  {
    Session theSession = Session.GetSession();
    Part workPart = theSession.Parts.Work;
    Part displayPart = theSession.Parts.Display;
	
	UFSession ufs = UFSession.GetUFSession();
	UI ui = UI.GetUI();
	LW = theSession.ListingWindow;
	LW.Open();	
	//string filename = CreateFirstDialog();
	//if (filename == "") return;
	//return;	
	txtfile = new System.IO.StreamWriter(@"C:\111\111234.txt", false);
	txtfile.AutoFlush = false;

	LW.WriteLine("START...");
	
	bool bNone;
	foreach (NXOpen.Drawings.DrawingSheet sheet in workPart.DrawingSheets)
	{
			PrintSTR("LISTNAME: " + sheet.Name);
			PrintSTR("LIST_WIDTH: " + sheet.Length.ToString());
			PrintSTR("LIST_HEIGHT: " + sheet.Height.ToString());
			PrintSTR("CREATELIST");
			
			
			foreach (NXOpen.Drawings.DraftingView view in sheet.SheetDraftingViews)
			{
				PrintSTR(view.Matrix.ToString());
				PrintSTR("VIEWNAME: " + view.Name);
				PrintSTR("VIEW_M: " + view.Scale.ToString());
				PrintSTR("CREATEVIEW");
				
				foreach (DisplayableObject obj in view.AskVisibleObjects())
				{
					bNone = false;
					if (obj is NXOpen.Point) 
					{
						NXOpen.Point point = (NXOpen.Point)obj;
						//lw.WriteLine("Line: " + point.Coordinates.X.ToString() + ", " + point.Coordinates.Y.ToString());
						PrintSTR(String.Format("Point: {0}; {1}; {2}", GetKompasObjStyle(obj), GetX(view, point.Coordinates), GetY(view, point.Coordinates)));
						bNone = true;
					}
					if (obj is NXOpen.Line) 
					{
						NXOpen.Line line = (NXOpen.Line)obj;
						PrintSTR(String.Format("LINE: {0}; {1}; {2}; {3}; {4}", GetKompasObjStyle(obj), GetX(view, line.StartPoint), GetY(view, line.StartPoint), GetX(view, line.EndPoint), GetY(view, line.EndPoint)));
						bNone = true;
					}
					if (obj is NXOpen.Arc) 
					{
						NXOpen.Arc arc = (NXOpen.Arc)obj;
						PrintSTR(String.Format("ARC: {0}; {1}; {2}; {3}; {4}; {5}", GetKompasObjStyle(obj), GetX(view, arc.CenterPoint), GetY(view, arc.CenterPoint), arc.Radius, arc.StartAngle, arc.EndAngle));
						bNone = true;
					}
					if (obj is NXOpen.Ellipse) 
					{
						NXOpen.Ellipse ellipse = (NXOpen.Ellipse)obj;
						PrintSTR(String.Format("ELLIPSE: {0}; {1}; {2}; {3}; {4}; {5}; {6}", GetKompasObjStyle(obj), GetX(view, ellipse.CenterPoint), GetY(view, ellipse.CenterPoint), ellipse.MinorRadius, ellipse.MajorRadius , ellipse.StartAngle, ellipse.EndAngle));
						bNone = true;
					}
					if (!bNone)
					{
						PrintSTR("\t\tОбъект: " + obj.GetType().Name);
					}
					
					
/*
LINE: 1, 100, 100, 200, 200				'стиль,x1,y1,x2,y2
CIRCLE: 1, 100, 100, 20					'стиль,x1,y1,r
ARC: 1, 100, 100, 200, 200, 20			'стиль,CenterX,CenterY,Radius,StartAngle,EndAngle
ELLIPSE: 1, 100, 100, 200, 200, 20		'стиль,CenterX,CenterY,MinorRadius,MajorRadius,StartAngle,EndAngle
	*/				
					
					/*if (OBJs[i].GetType().Name == "Edge")
					{
						//OBJs[i].Blank();
						//OBJs[i].Unhighlight();
						//DraftingCurve  l;// = (DraftingCurve)OBJs[i];

						ICurve ttt = (ICurve)OBJs[i];
						lw.WriteLine("L = " + ttt.GetLength().ToString());
						//break;
					}*/

					
				}
			
			}
	}
	
	txtfile.Close();
	
	LW.WriteLine("END");
	LW.Close();

 
  }
  public static int GetUnloadOption(string dummy) { return (int)Session.LibraryUnloadOption.Immediately; }
}
