// NX 9.0.3.4
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
		if (obj.LineWidth == DisplayableObject.ObjectWidth.One) return 0;
		if (obj.LineWidth == DisplayableObject.ObjectWidth.Two) return 1;
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
	
	//Матрица поворота вида и матрица поворота дуги независимы друг от друга, и отсчитываются только от начальной системы координат
	//=> угол поворота матрицы дуги относительно матрицы вида равен (матрица вида)*(обратная матрица дуги)
	//http://matrixcalc.org/	- онлайн расчёт матриц
	//http://blog2k.ru/archives/3197	- блок схема для определения поворота
	//http://en.wikipedia.org/wiki/Rotation_matrix	- касательно матриц поворота (с примерами)
	//http://en.wikipedia.org/wiki/Axis%E2%80%93angle_representation#Log_map_from_SO.283.29_to_so.283.29	- доп.материал
	public static int GetMatrixDirection4(UFSession ufs, NXOpen.Matrix3x3 viewM, NXOpen.Matrix3x3 arcM)		//Направление
	{
		//считаем что nx нас не обманывает и определители матриц равны 1
		//считаем что вектор Z вида и вектор Z дуги 	коллинеарны
		//за направление следует принять один из векторов Z

		double [] vecX_view = {viewM.Xx, viewM.Xy, viewM.Xz};
		double [] vecZ_view = {viewM.Zx, viewM.Zy, viewM.Zz};
		double [] vecZ_arc  = {arcM.Zx,  arcM.Zy,  arcM.Zz};
		double angZ;
		ufs.Vec3.AngleBetween(vecZ_view, vecZ_arc, vecX_view, out angZ);
		if (angZ > 0.000001) return -1;
		return 1;
	}
	
	public static double convertANG_ABStoCSYS(UFSession ufs, double ang, NXOpen.Matrix3x3 viewM, NXOpen.Matrix3x3 arcM)		//Направление и угол (для дуг и элипсов)
	{
		double [] uf_viewM = convert_Matrix3x3_to_UFMatrix(viewM);
		double [] uf_arcM  = convert_Matrix3x3_to_UFMatrix(arcM);
		
		double [] model_pt = {Math.Cos(ang), Math.Sin(ang), 0};
		double [] result = {0,0,0};
		ufs.Mtx3.VecMultiplyT(model_pt, uf_arcM, result);
		ufs.Mtx3.VecMultiply(result, uf_viewM, result);
		//PrintSTR("x = " + result[0] + "\t\t Acos=" + Math.Acos(result[0])*180/Math.PI+ "\t Asin=" + Math.Asin(result[0])*180/Math.PI);
		//PrintSTR("y = " + result[1] + "\t\t Acos=" + Math.Acos(result[1])*180/Math.PI+ "\t Asin=" + Math.Asin(result[1])*180/Math.PI);
		//PrintSTR("z = " + result[2] + "\t\t Acos=" + Math.Acos(result[2])*180/Math.PI+ "\t Asin=" + Math.Asin(result[2])*180/Math.PI);
		int znak = 1;
		if (result[1] < 0) znak = -1;
		return znak * Math.Acos(result[0]);
	}
	
	public static double [] convert_Matrix3x3_to_UFMatrix(NXOpen.Matrix3x3 M)
	{
		double [] result = {M.Xx, M.Xy, M.Xz,     M.Yx, M.Yy, M.Yz,     M.Zx, M.Zy, M.Zz};
		return result;
	}
	
	public static NXOpen.Point3d convertCoord_CSYStoABS(UFSession ufs, NXOpen.Point3d point, NXOpen.Matrix3x3 viewM)
	{
		double [] uf_viewM = convert_Matrix3x3_to_UFMatrix(viewM);
		double [] model_pt = {point.X, point.Y, point.Z};
		double [] result = {0,0,0};
		ufs.Mtx3.VecMultiply(model_pt, uf_viewM, result);
		//PrintSTR(String.Format("\t\tconvertLineCoord: {0}; {1}; {2}",result[0],result[1],result[2]));
		return new NXOpen.Point3d(result[0], result[1], result[2]);
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
				Point3d ViewPoint = view.GetDrawingReferencePoint();
				PrintSTR(String.Format("CREATEVIEW: \"{0}\"; {1}; {2}; {3}", view.Name, view.Style.General.Scale, ViewPoint.X, ViewPoint.Y));	//view.Scale отвечает за отображение на экране
				PrintSTR(view.Matrix.ToString());
				//PrintSTR(sheet.View.AskVisibleObjects().Length.ToString());
				Tag tag = Tag.Null;
				TaggedObject obj;
				
				do
				{
					ufs.View.CycleObjects(view.Tag, UFView.CycleObjectsEnum.DependentObjects, ref tag);
					if (tag == Tag.Null) break;

					obj = NXOpen.Utilities.NXObjectManager.Get(tag);
					bNone = false;
					//PrintSTR(obj.ToString());

					
					if (obj is NXOpen.Point)
					{
						NXOpen.Point point = (NXOpen.Point)obj;
						//PrintSTR(String.Format("Point: {0}; {1}; {2}", GetKompasObjStyle(obj), GetX(view, point.Coordinates), GetY(view, point.Coordinates)));
						bNone = true;
					}
					if (obj is NXOpen.Line) 
					{
						NXOpen.Line line = (NXOpen.Line)obj;
						bNone = true;
						NXOpen.Point3d startP = convertCoord_CSYStoABS(ufs, line.StartPoint, view.Matrix);
						NXOpen.Point3d   endP = convertCoord_CSYStoABS(ufs, line.EndPoint,   view.Matrix);
						//PrintSTR(String.Format("LINE: {0}; {1}; {2}; {3}; {4}", GetKompasObjStyle(obj), startP.X, startP.Y, endP.X, endP.Y));
					}
					if (obj is NXOpen.Arc) 
					{
						NXOpen.Arc arc = (NXOpen.Arc)obj;
						bNone = true;
						NXOpen.Point3d centerP = convertCoord_CSYStoABS(ufs, arc.CenterPoint,   view.Matrix);
						double StartAng = convertANG_ABStoCSYS(ufs, arc.StartAngle, view.Matrix, arc.Matrix.Element);
						double EngAng = convertANG_ABStoCSYS(ufs, arc.EndAngle, view.Matrix, arc.Matrix.Element);
						PrintSTR(String.Format("ARC: {0}; {1}; {2}; {3}; {4}; {5}; {6}", GetKompasObjStyle(obj), centerP.X, centerP.Y, arc.Radius, StartAng, EngAng,    GetMatrixDirection4(ufs, view.Matrix, arc.Matrix.Element)    ));
//PrintSTR(arc.Matrix.Element.ToString());
//GetMatrixDirection4(ufs, arc.StartAngle, view.Matrix, arc.Matrix.Element);
//GetMatrixDirection4(ufs, arc.EndAngle, view.Matrix, arc.Matrix.Element);
//PrintSTR(GetMatrixDirection(view.Matrix, arc.Matrix.Element).ToString());
					}
					if (obj is NXOpen.Ellipse) 
					{
						NXOpen.Ellipse ellipse = (NXOpen.Ellipse)obj;
						NXOpen.Point3d centerP = convertCoord_CSYStoABS(ufs, ellipse.CenterPoint,   view.Matrix);
						//PrintSTR(String.Format("ELLIPSE: {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}", GetKompasObjStyle(obj), centerP.X, centerP.Y, ellipse.MinorRadius, ellipse.MajorRadius , ellipse.StartAngle, ellipse.EndAngle,   GetMatrixDirection3(ufs, view.Matrix, ellipse.Matrix.Element)));
						//PrintSTR(ellipse.Matrix.Element.ToString());
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
CREATEVIEW: "Front@23"; 0,01, 100, 200		имя, масштаб, начало вида X, начало вида Y
Point: 1; 0; 0							'x,y
LINE: 1, 100, 100, 200, 200				'стиль,x1,y1,x2,y2
ARC: 1, 100, 100, 200, 200, 20, -1		'стиль,CenterX,CenterY,Radius,StartAngle,EndAngle,Direction
ELLIPSE: 1, 100, 100, 200, 200, 200, 20, -90	'стиль,CenterX,CenterY,MinorRadius,MajorRadius,StartAngle,EndAngle,Direction
	*/				
					
			}
	}
	
	txtfile.Close();
	
	LW.WriteLine("Всего объектов на чертеже: " + countAllObjs.ToString());
	LW.WriteLine("END");
	LW.Close();

 
  }
  public static int GetUnloadOption(string dummy) { return (int)Session.LibraryUnloadOption.Immediately; }
}
