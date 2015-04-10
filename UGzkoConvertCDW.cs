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
		if (obj.LineWidth == DisplayableObject.ObjectWidth.One) return 1;
		if (obj.LineWidth == DisplayableObject.ObjectWidth.Two) return 2;
		return 0;
	}
	public static int GetKompasObjStyle(TaggedObject obj)
	{
		return GetKompasObjStyle((DisplayableObject)obj);
	}
	
	public static void PrintSTR(string s)
	{
		LW.WriteLine(s);
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
	
	/*public static double GetMatrixDirection(NXOpen.NXMatrix matrix)		//Направление матрицы (для дуг направление вращения)
	{
	//PrintSTR(view.Matrix.
	PrintSTR(String.Format("MATRIX x={0}  y={1}  z={2}", matrix.Zx , matrix.Zy , matrix.Zz));
		return matrix.Zx + matrix.Zy + matrix.Zz;		//либо 1 либо -1
	}*/
	
	public static double GetMatrixDirection(NXOpen.Matrix3x3 viewM, NXOpen.Matrix3x3 arcM)		//Направление матрицы (для дуг направление вращения)
	{
		//в функции принято что вращается только XY
		//координата Z фиксирована, может быть + или -, от неё зависит направление движения
		//угол поворота находится по одной координате (из свойств поворотных матриц)
		double EPS = 0.000001;	//погрешность вычислений
		double Xview, Zview;
		if (viewM.Zx != 0)	{	Xview = viewM.Xy;			Zview = viewM.Zx;	}
		if (viewM.Zy != 0)	{	Xview = viewM.Xx;			Zview = viewM.Zy;	}
		if (viewM.Zz != 0)	{	Xview = viewM.Xx;			Zview = viewM.Zz;	}
		
		double Xarc, Zarc;
		if (arcM.Zx != 0)	{	Xarc = arcM.Xy;				Zarc = arcM.Zx;		}
		if (arcM.Zy != 0)	{	Xarc = arcM.Xx;				Zarc = arcM.Zy;		}
		if (arcM.Zz != 0)	{	Xarc = arcM.Xx;				Zarc = arcM.Zz;		}
		
		double Gview = Math.Acos(Xview) / Math.PI * 180;
		double Garc  = Math.Acos(Xarc)  / Math.PI * 180;
		
		//if (Math.Abs(Gview - 180) < EPS) 	Gview = 0;
		//if (Math.Abs(Garc - 180) < EPS) 	Garc = 0;
		
		if (Math.Abs(Zview - Zarc) < EPS) return Zview - Zarc; else return -(Zview - Zarc);
	}
	
	
  public static void Main(string[] args)
  {
	int countAllObjs = 0;
	
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
	txtfile = new System.IO.StreamWriter(@"d:\2d.txt", false);
	txtfile.AutoFlush = false;

	LW.WriteLine("START...");
	
	bool bNone;
	foreach (NXOpen.Drawings.DrawingSheet sheet in workPart.DrawingSheets)			//sheet.View объекты которые входят только в чертеж
	{
			PrintSTR(String.Format("CREATELIST: \"{0}\"; {1}; {2}", sheet.Name, sheet.Length, sheet.Height));

			foreach (NXOpen.Drawings.DraftingView view in sheet.SheetDraftingViews)
			{
				PrintSTR(String.Format("CREATEVIEW: \"{0}\"; {1}", view.Name, view.Style.General.Scale));	//view.Scale отвечает за отображение на экране
PrintSTR(view.Matrix.ToString());
				//PrintSTR(sheet.View.AskVisibleObjects().Length.ToString());
				//PrintSTR(sheet.SheetDraftingViews.ToArray().Length.ToString());
				Tag tag = Tag.Null;
				TaggedObject obj;
				
				do
				{
					ufs.View.CycleObjects(view.Tag, UFView.CycleObjectsEnum.DependentObjects, ref tag);
					if (tag == Tag.Null) break;

					obj = NXOpen.Utilities.NXObjectManager.Get(tag);
					bNone = false;
					//PrintSTR(obj.ToString());
					//ufs.Trns.CreateRotationMatrix
					 
					
					
					
					
					if (obj is NXOpen.Point)
					{
						NXOpen.Point point = (NXOpen.Point)obj;
						//lw.WriteLine("Line: " + point.Coordinates.X.ToString() + ", " + point.Coordinates.Y.ToString());
						//PrintSTR(String.Format("Point: {0}; {1}; {2}", GetKompasObjStyle(obj), GetX(view, point.Coordinates), GetY(view, point.Coordinates)));
						bNone = true;
					}
					if (obj is NXOpen.Line) 
					{
						NXOpen.Line line = (NXOpen.Line)obj;
						//PrintSTR(String.Format("LINE: {0}; {1}; {2}; {3}; {4}", GetKompasObjStyle(obj), GetX(view, line.StartPoint), GetY(view, line.StartPoint), GetX(view, line.EndPoint), GetY(view, line.EndPoint)));
						bNone = true;
					}
					if (obj is NXOpen.Arc) 
					{
						NXOpen.Arc arc = (NXOpen.Arc)obj;
						PrintSTR(String.Format("ARC: {0}; {1}; {2}; {3}; {4}; {5}; {6}", GetKompasObjStyle(obj), GetX(view, arc.CenterPoint), GetY(view, arc.CenterPoint), arc.Radius, arc.StartAngle, arc.EndAngle,    (arc.Matrix.Element.Zx+arc.Matrix.Element.Zy+arc.Matrix.Element.Zz)           ));
						bNone = true;
						//10465
						PrintSTR(arc.RotationAngle.ToString());
						PrintSTR(arc.Matrix.Element.ToString());
						
					}
					if (obj is NXOpen.Ellipse) 
					{
						NXOpen.Ellipse ellipse = (NXOpen.Ellipse)obj;
						//PrintSTR(String.Format("ELLIPSE: {0}; {1}; {2}; {3}; {4}; {5}; {6}", GetKompasObjStyle(obj), GetX(view, ellipse.CenterPoint), GetY(view, ellipse.CenterPoint), ellipse.MinorRadius, ellipse.MajorRadius , ellipse.StartAngle, ellipse.EndAngle));
						bNone = true;
					}
					if (!bNone)
					{
						//PrintSTR("NONE: " + obj.GetType().Name);
					}
					countAllObjs++;
				} while (tag != Tag.Null);


/*
CREATELIST: "Sheet 4"; 1189; 841				имя, длина, высота
CREATEVIEW: "Front@23"; 0,00474517367052386		имя, масштаб
Point: 1; 0; 0							'x,y
LINE: 1, 100, 100, 200, 200				'стиль,x1,y1,x2,y2
ARC: 1, 100, 100, 200, 200, 20			'стиль,CenterX,CenterY,Radius,StartAngle,EndAngle,Direction
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
	
	txtfile.Close();
	
	LW.WriteLine("Всего объектов на чертеже: " + countAllObjs.ToString());
	LW.WriteLine("END");
	LW.Close();

 
  }
  public static int GetUnloadOption(string dummy) { return (int)Session.LibraryUnloadOption.Immediately; }
}
