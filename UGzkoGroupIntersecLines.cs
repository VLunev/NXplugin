using System;
using System.Collections;
using NXOpen;
using NXOpen.BlockStyler;

//------------------------------------------------------------------------------
//Класс пересечения линий 3D взять из UGzkoIntersecLines.cs 	V1.0
//------------------------------------------------------------------------------
public class LinesIntersection
{
	public struct Vector3
	{
		public double X;
		public double Y;
		public double Z;
		public Vector3(double x, double y, double z)
		{
			this.X = x; this.Y = y; this.Z = z;
		}
		
		public static Vector3 operator-(Vector3 v1, Vector3 v2 )
		{
			return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		}
		
		public static Vector3 operator+(Vector3 v1, Vector3 v2 )
		{
			return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}
		
		public static Vector3 operator*(Vector3 v1, Vector3 v2 )
		{
			return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
		}
		
		public static Vector3 operator*(Vector3 v1, double d )
		{
			return new Vector3(v1.X * d, v1.Y * d, v1.Z * d);
		}
					
		public static double operator^(Vector3 v1, Vector3 v2)	//dot (скалярное произведение векторов)  суммирование всех перемноженных членов(сделано для удобства, можно вывести в отдельную функцию)
		{
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
		}
		public double Length()
		{
			return Math.Sqrt(this^this);
		}	
		
		
		
	}

	public static bool LineLineIntersection(Vector3 line1Point1, Vector3 line1Point2, Vector3 line2Point1, Vector3 line2Point2, out Vector3 resultSegmentPoint1, out Vector3 resultSegmentPoint2)
	{
		Vector3 u = line1Point2 - line1Point1;
		Vector3 v = line2Point2 - line2Point1;
		Vector3 w = line1Point1 - line2Point1;
		double    a = u^u;         // always >= 0
		double    b = u^v;
		double    c = v^v;         // always >= 0
		double    d = u^w;
		double    e = v^w;
		double    D = a*c - b*b;   // always >= 0
		double    sc, tc;
		const double EPS = 0.00001;

		if (D < EPS) 
		//if (Math.Abs(b) > EPS)
		{          						// the lines are almost parallel
			//sc = 0.0;
			//tc = (b>c ? d/b : e/c);     // use the largest denominator
			resultSegmentPoint1 = new Vector3(0,0,0);
			resultSegmentPoint2 = new Vector3(0,0,0);
			return false;
		}
		else
		{
			sc = (b*e - c*d) / D;
			tc = (a*e - b*d) / D;
		}

		resultSegmentPoint1 = line1Point1 + (u * sc);
		resultSegmentPoint2 = line2Point1 + (v * tc);
		Vector3  dP = w + ((u * sc) - (v * tc));  // =  L1(sc) - L2(tc)

		return true;
	}
	
	public double NXOpenLineLineIntersection(Point3d line1Point1, Point3d line1Point2, Point3d line2Point1, Point3d line2Point2, out Point3d Point1, out Point3d Point2)
	{
		Vector3 P1L1 = new Vector3(line1Point1.X, line1Point1.Y, line1Point1.Z);
		Vector3 P2L1 = new Vector3(line1Point2.X, line1Point2.Y, line1Point2.Z);
		Vector3 P1L2 = new Vector3(line2Point1.X, line2Point1.Y, line2Point1.Z);
		Vector3 P2L2 = new Vector3(line2Point2.X, line2Point2.Y, line2Point2.Z);
		Vector3 P1 = new Vector3(0, 0, 0);
		Vector3 P2 = new Vector3(0, 0, 0);
		
		bool result = LineLineIntersection(P1L1, P2L1, P1L2, P2L2, out P1, out P2);
		
		Point1 = new Point3d(P1.X, P1.Y, P1.Z);
		Point2 = new Point3d(P2.X, P2.Y, P2.Z);

		if (!result) return -1;
		return (P1 - P2).Length();
	}
	
	public double NXOpenLineLineIntersection(NXOpen.Routing.LineSegment line1, NXOpen.Routing.LineSegment line2, out Point3d Point1, out Point3d Point2)
	{
		Point3d line1Point1, line1Point2;
		Point3d line2Point1, line2Point2;
		line1.GetEndPoints(out line1Point1, out line1Point2);
		line2.GetEndPoints(out line2Point1, out line2Point2);
		
		Vector3 P1L1 = new Vector3(line1Point1.X, line1Point1.Y, line1Point1.Z);
		Vector3 P2L1 = new Vector3(line1Point2.X, line1Point2.Y, line1Point2.Z);
		Vector3 P1L2 = new Vector3(line2Point1.X, line2Point1.Y, line2Point1.Z);
		Vector3 P2L2 = new Vector3(line2Point2.X, line2Point2.Y, line2Point2.Z);
		Vector3 P1 = new Vector3(0, 0, 0);
		Vector3 P2 = new Vector3(0, 0, 0);
		
		LineLineIntersection(P1L1, P2L1, P1L2, P2L2, out P1, out P2);
		
		Point1 = new Point3d(P1.X, P1.Y, P1.Z);
		Point2 = new Point3d(P2.X, P2.Y, P2.Z);
		return (P1 - P2).Length();
	}
	
	public double GetDistancePointPoint(Point3d P1, Point3d P2)
	{
		return Math.Sqrt((P1.X - P2.X)*(P1.X - P2.X) + (P1.Y - P2.Y)*(P1.Y - P2.Y) + (P1.Z - P2.Z)*(P1.Z - P2.Z));
	}
}

//------------------------------------------------------------------------------
//Represents Block Styler application class
//------------------------------------------------------------------------------
public class UGzkoGroupIntersecLines
{
	//Мои переменные
	LinesIntersection LI;
    //class members
    private static Session theSession = null;
	private Part workPart;
    private static UI theUI = null;
    private string theDlxFileName;
    private NXOpen.BlockStyler.BlockDialog theDialog;
    private NXOpen.BlockStyler.Group group0;// Block type: Group
    private NXOpen.BlockStyler.SelectObject selection0;// Block type: Selection
    private NXOpen.BlockStyler.SelectObject selection1;// Block type: Selection
    private NXOpen.BlockStyler.SpecifyPoint point0;// Block type: Specify Point
    private NXOpen.BlockStyler.SpecifyPoint point1;// Block type: Specify Point
    //------------------------------------------------------------------------------
    //Bit Option for Property: SnapPointTypesEnabled
    //------------------------------------------------------------------------------
    public static readonly int              SnapPointTypesEnabled_UserDefined = (1 << 0);
    public static readonly int                 SnapPointTypesEnabled_Inferred = (1 << 1);
    public static readonly int           SnapPointTypesEnabled_ScreenPosition = (1 << 2);
    public static readonly int                 SnapPointTypesEnabled_EndPoint = (1 << 3);
    public static readonly int                 SnapPointTypesEnabled_MidPoint = (1 << 4);
    public static readonly int             SnapPointTypesEnabled_ControlPoint = (1 << 5);
    public static readonly int             SnapPointTypesEnabled_Intersection = (1 << 6);
    public static readonly int                SnapPointTypesEnabled_ArcCenter = (1 << 7);
    public static readonly int            SnapPointTypesEnabled_QuadrantPoint = (1 << 8);
    public static readonly int            SnapPointTypesEnabled_ExistingPoint = (1 << 9);
    public static readonly int             SnapPointTypesEnabled_PointonCurve = (1 <<10);
    public static readonly int           SnapPointTypesEnabled_PointonSurface = (1 <<11);
    public static readonly int         SnapPointTypesEnabled_PointConstructor = (1 <<12);
    public static readonly int     SnapPointTypesEnabled_TwocurveIntersection = (1 <<13);
    public static readonly int             SnapPointTypesEnabled_TangentPoint = (1 <<14);
    public static readonly int                    SnapPointTypesEnabled_Poles = (1 <<15);
    public static readonly int         SnapPointTypesEnabled_BoundedGridPoint = (1 <<16);
    public static readonly int         SnapPointTypesEnabled_FacetVertexPoint = (1 <<17);
    //------------------------------------------------------------------------------
    //Bit Option for Property: SnapPointTypesOnByDefault
    //------------------------------------------------------------------------------
    public static readonly int          SnapPointTypesOnByDefault_UserDefined = (1 << 0);
    public static readonly int             SnapPointTypesOnByDefault_Inferred = (1 << 1);
    public static readonly int       SnapPointTypesOnByDefault_ScreenPosition = (1 << 2);
    public static readonly int             SnapPointTypesOnByDefault_EndPoint = (1 << 3);
    public static readonly int             SnapPointTypesOnByDefault_MidPoint = (1 << 4);
    public static readonly int         SnapPointTypesOnByDefault_ControlPoint = (1 << 5);
    public static readonly int         SnapPointTypesOnByDefault_Intersection = (1 << 6);
    public static readonly int            SnapPointTypesOnByDefault_ArcCenter = (1 << 7);
    public static readonly int        SnapPointTypesOnByDefault_QuadrantPoint = (1 << 8);
    public static readonly int        SnapPointTypesOnByDefault_ExistingPoint = (1 << 9);
    public static readonly int         SnapPointTypesOnByDefault_PointonCurve = (1 <<10);
    public static readonly int       SnapPointTypesOnByDefault_PointonSurface = (1 <<11);
    public static readonly int     SnapPointTypesOnByDefault_PointConstructor = (1 <<12);
    public static readonly int SnapPointTypesOnByDefault_TwocurveIntersection = (1 <<13);
    public static readonly int         SnapPointTypesOnByDefault_TangentPoint = (1 <<14);
    public static readonly int                SnapPointTypesOnByDefault_Poles = (1 <<15);
    public static readonly int     SnapPointTypesOnByDefault_BoundedGridPoint = (1 <<16);
    public static readonly int     SnapPointTypesOnByDefault_FacetVertexPoint = (1 <<17);

    //------------------------------------------------------------------------------
    //Constructor for NX Styler class
    //------------------------------------------------------------------------------
    public UGzkoGroupIntersecLines()
    {
        try
        {
            theSession = Session.GetSession();
			workPart = theSession.Parts.Work;
            theUI = UI.GetUI();
            theDlxFileName = "D:\\NXJournal\\NXplugin\\DLX\\UGzkoGroupIntersecLines.dlx";
            theDialog = theUI.CreateDialog(theDlxFileName);
            theDialog.AddOkHandler(new NXOpen.BlockStyler.BlockDialog.Ok(ok_cb));
            theDialog.AddFilterHandler(new NXOpen.BlockStyler.BlockDialog.Filter(filter_cb));
            theDialog.AddInitializeHandler(new NXOpen.BlockStyler.BlockDialog.Initialize(initialize_cb));
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            throw ex;
        }
    }
    //------------------------------- DIALOG LAUNCHING ---------------------------------
    public static void Main()
    {
        UGzkoGroupIntersecLines theUGzkoGroupIntersecLines = null;
        try
        {
            theUGzkoGroupIntersecLines = new UGzkoGroupIntersecLines();
            // The following method shows the dialog immediately
            theUGzkoGroupIntersecLines.Show();
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        finally
        {
            if(theUGzkoGroupIntersecLines != null)
                theUGzkoGroupIntersecLines.Dispose();
                theUGzkoGroupIntersecLines = null;
        }
    }
    //------------------------------------------------------------------------------
    // This method specifies how a shared image is unloaded from memory
    // within NX
    //------------------------------------------------------------------------------
     public static int GetUnloadOption(string arg)
    {
         return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
    }
    
    //------------------------------------------------------------------------------
    // Following method cleanup any housekeeping chores that may be needed.
    // This method is automatically called by NX.
    //------------------------------------------------------------------------------
    public static void UnloadLibrary(string arg)
    {
        try
        {
            //---- Enter your code here -----
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
    }
    
    //------------------------------------------------------------------------------
    //This method shows the dialog on the screen
    //------------------------------------------------------------------------------
    public NXOpen.UIStyler.DialogResponse Show()
    {
        try
        {
            theDialog.Show();
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        return 0;
    }
    
    //------------------------------------------------------------------------------
    //Method Name: Dispose
    //------------------------------------------------------------------------------
    public void Dispose()
    {
        if(theDialog != null)
        {
            theDialog.Dispose();
            theDialog = null;
        }
    }
    
    //------------------------------------------------------------------------------
    //---------------------Block UI Styler Callback Functions--------------------------
    //------------------------------------------------------------------------------
    
    //------------------------------------------------------------------------------
    //Callback Name: initialize_cb
    //------------------------------------------------------------------------------
    public void initialize_cb()
    {
        try
        {
            group0 = (NXOpen.BlockStyler.Group)theDialog.TopBlock.FindBlock("group0");
            selection0 = (NXOpen.BlockStyler.SelectObject)theDialog.TopBlock.FindBlock("selection0");
            selection1 = (NXOpen.BlockStyler.SelectObject)theDialog.TopBlock.FindBlock("selection1");
            point0 = (NXOpen.BlockStyler.SpecifyPoint)theDialog.TopBlock.FindBlock("point0");
            point1 = (NXOpen.BlockStyler.SpecifyPoint)theDialog.TopBlock.FindBlock("point1");
			LI = new LinesIntersection();
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
    }
    
   
    //------------------------------------------------------------------------------
    //Callback Name: ok_cb
    //------------------------------------------------------------------------------
		public class SortingLinesClass : IComparer  
		{
			private Point3d BeginPoint;
			public SortingLinesClass(Point3d p)
			{
				BeginPoint = p;
			}
			private double GetDistancePointPoint(Point3d P1, Point3d P2)
			{
				return Math.Sqrt((P1.X - P2.X)*(P1.X - P2.X) + (P1.Y - P2.Y)*(P1.Y - P2.Y) + (P1.Z - P2.Z)*(P1.Z - P2.Z));
			}
			int IComparer.Compare( Object obj1, Object obj2 )  
			{
				Line line1 = (Line)obj1;
				Line line2 = (Line)obj2;
				//остаётся необработанный вариант когда 2 точки расположены по разным сторонам от начальной точки
				
				Point3d p1, p2;	//контрольные точки для линий 1 и 2 соответственно
				
				if (GetDistancePointPoint(BeginPoint, line1.StartPoint) < GetDistancePointPoint(BeginPoint, line1.EndPoint)) p1 = line1.StartPoint; else p1 = line1.EndPoint;
				if (GetDistancePointPoint(BeginPoint, line2.StartPoint) < GetDistancePointPoint(BeginPoint, line2.EndPoint)) p2 = line2.StartPoint; else p2 = line2.EndPoint;
				double L1 = GetDistancePointPoint(BeginPoint, p1); //минимальное расстояние до линии 1
				double L2 = GetDistancePointPoint(BeginPoint, p2); //минимальное расстояние до линии 2
				if ((L1 - L2) > 0.00001)  return 1;
				if ((L1 - L2) < -0.00001) return -1;
				return 0;
			}
		}
		
    public int ok_cb()
    {
        int errorCode = 0;
        try
        {
			/*ListingWindow lw = theSession.ListingWindow;
			lw.Open();			
			string[] S = SelectPropsPoint0.GetPropertyNames();
			foreach(string str in S) lw.WriteLine(str);*/	
			
			
		//Получение сегментов группы 1
			NXOpen.BlockStyler.PropertyList SelectPropsSelection0 = selection0.GetProperties();
			TaggedObject[] selectedLines1 = SelectPropsSelection0.GetTaggedObjectVector("SelectedObjects");
		//Получение сегментов группы 2
			NXOpen.BlockStyler.PropertyList SelectPropsSelection1 = selection1.GetProperties(); 
			TaggedObject[] selectedLines2 = SelectPropsSelection1.GetTaggedObjectVector("SelectedObjects");
		//Получение начальной точки группы 1
			NXOpen.BlockStyler.PropertyList SelectPropsPoint0 = point0.GetProperties();
			Point3d BeginPoint1 = (Point3d)SelectPropsPoint0.GetPoint("Point");
		//Получение начальной точки группы 2
			NXOpen.BlockStyler.PropertyList SelectPropsPoint1 = point1.GetProperties();
			Point3d BeginPoint2 = (Point3d)SelectPropsPoint1.GetPoint("Point");
		//Сортировка массива сегментов группы 1
			IComparer sortClass1 = new SortingLinesClass(BeginPoint1);
			Array.Sort(selectedLines1, sortClass1);
		//Сортировка массива сегментов группы 2
			IComparer sortClass2 = new SortingLinesClass(BeginPoint2);
			Array.Sort(selectedLines2, sortClass2);
		//
			if (selectedLines1.Length != selectedLines2.Length) 
			{
				theUI.NXMessageBox.Show("Предупреждение", NXMessageBox.DialogType.Error, "Количество сегментов группы 1 не совпадает с группой 2");
				return 1;
			}
			for (int i = 0; i < selectedLines1.Length; i++)
				CreatePATH(selectedLines1[i], selectedLines2[i]);
        }
        catch (Exception ex)
        {
            errorCode = 1;
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        return errorCode;
    }
    
    //------------------------------------------------------------------------------
    //Callback Name: filter_cb
    //------------------------------------------------------------------------------
    public int filter_cb(NXOpen.BlockStyler.UIBlock block, NXOpen.TaggedObject selectedObject)
    {
		int accept = NXOpen.UF.UFConstants.UF_UI_SEL_REJECT;//UF_UI_SEL_ACCEPT;
		try
		{
			if ((block == selection0) && (selectedObject is NXOpen.Routing.LineSegment)) accept = NXOpen.UF.UFConstants.UF_UI_SEL_ACCEPT;
			if ((block == selection1) && (selectedObject is NXOpen.Routing.LineSegment)) accept = NXOpen.UF.UFConstants.UF_UI_SEL_ACCEPT;
			if ((block == point0) || (block == point1)) accept = NXOpen.UF.UFConstants.UF_UI_SEL_ACCEPT;
        }
        catch (Exception ex)
        {
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        return accept;
    }
    



	//------------------------------------------------------------------------------
	//Создание сегментов маршрута по заданным точкам
	//------------------------------------------------------------------------------
	public bool CreateLines2(Point3d[] points, NXOpen.TaggedObject BeginObj, NXOpen.TaggedObject EndObj) //если точка попадает на BeginObj или EndObj, то происходит разделение сегмента если это  LineSegment
	{
		NXOpen.Routing.LinearPathBuilder linearPathBuilder1;
		NXOpen.Routing.PathStockBuilder pathStockBuilder1;
		NXOpen.Routing.LinearPathSettings linearPathSettings1;

		pathStockBuilder1 = workPart.RouteManager.CreatePathStockBuilder();
		linearPathBuilder1 = workPart.RouteManager.CreateLinearPathBuilder();
		linearPathSettings1 = workPart.RouteManager.CreateLinearPathSettings();

		linearPathBuilder1.PathStockBuilder = pathStockBuilder1;
		linearPathBuilder1.LinearPathSettings = linearPathSettings1;

		pathStockBuilder1.StockType = NXOpen.Routing.PathStockBuilder.AssignStockType.Stock;

		linearPathBuilder1.SettingChanged();

		for (int i = 0; i < points.Length; i++)
		{
			//theUI.NXMessageBox.Show("Ошибка", NXMessageBox.DialogType.Error, points[i].ToString() );
			NXOpen.Routing.ControlPoint controlPoint1 = linearPathBuilder1.AddPreviewControlPoint(points[i]);
			if (i == 0)
			{
				linearPathBuilder1.SetControlPointDefiningObject(controlPoint1, points[i], (NXObject)BeginObj); //При непосредственном пересечении разделяется только один отрезок (как и в UG)
			}
			else
			{
				if (i == points.Length - 1)
					linearPathBuilder1.SetControlPointDefiningObject(controlPoint1, points[i], (NXObject)EndObj);	
				else
					linearPathBuilder1.SetControlPointDefiningObject(controlPoint1, points[i], null);
			}
		};

		linearPathBuilder1.Commit();
		linearPathBuilder1.GetCommittedObjects();

		linearPathBuilder1.Destroy();
		linearPathSettings1.Destroy();
		//pathStockBuilder1.Destroy();
		return true;
	}
	//------------------------------------------------------------------------------
	//Построение кратчайшего пути  V1.0
	//------------------------------------------------------------------------------
	public bool CreatePATH(NXOpen.TaggedObject selObj1, NXOpen.TaggedObject selObj2)
	{
		//NXOpen.Routing.LineSegment LineSeg1 = (NXOpen.Routing.LineSegment)selObj1;
		//NXOpen.Routing.LineSegment LineSeg2 = (NXOpen.Routing.LineSegment)selObj2;
		//получение крайних точек линий
		//LineSeg1.GetEndPoints(out LineSeg1Point1, out LineSeg1Point2);
		//LineSeg2.GetEndPoints(out LineSeg2Point1, out LineSeg2Point2);	//не поддерживается лицензией

		Point3d[] points = null;
		Point3d P1, P2;				//точки отрезка пересечения
		Line line1 = (Line)selObj1;	//найден другой способ
		Line line2 = (Line)selObj2;
		
		//нахождение пересечения
		double LenghtIntersec = LI.NXOpenLineLineIntersection(line1.StartPoint, line1.EndPoint, line2.StartPoint, line2.EndPoint, out P1, out P2);
		//построение пересечения
		NXOpen.Session.UndoMarkId markId1 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Visible, "Start route intersec");
		
		if (LenghtIntersec == -1) 
		{
			//theUI.NXMessageBox.Show("Ошибка", NXMessageBox.DialogType.Error, "Линии параллельны");
			double L1 = LI.GetDistancePointPoint(line1.StartPoint, line2.StartPoint);
			double L2 = LI.GetDistancePointPoint(line1.StartPoint, line2.EndPoint);
			double L3 = LI.GetDistancePointPoint(line1.EndPoint,   line2.StartPoint);
			double L4 = LI.GetDistancePointPoint(line1.EndPoint,   line2.EndPoint);
			double [] ArrL = {L1, L2, L3, L4};
			Array.Sort(ArrL);
			if (ArrL[0] == L1)	{P1 = line1.StartPoint;		P2 = line2.StartPoint;};		//P1 P2 взяты как временные точки
			if (ArrL[0] == L2)	{P1 = line1.StartPoint;		P2 = line2.EndPoint;};
			if (ArrL[0] == L3)	{P1 = line1.EndPoint;		P2 = line2.StartPoint;};
			if (ArrL[0] == L4)	{P1 = line1.EndPoint;		P2 = line2.EndPoint;};

			points = new Point3d[2]{P1, P2};
			CreateLines2(points, selObj1, selObj2);
			return true;
		}
		
		Point3d IntersecPoint1, IntersecPoint2;
		bool BetweenPoint1, BetweenPoint2;
		//Определение точек начала и конца пути
		if (LI.GetDistancePointPoint(line1.StartPoint, P1) < LI.GetDistancePointPoint(line1.EndPoint, P1))  IntersecPoint1 = line1.StartPoint; 		//IntersecPoint это ближайшая точка откуда строится путь
																									else    IntersecPoint1 = line1.EndPoint;
		if (LI.GetDistancePointPoint(line2.StartPoint, P2) < LI.GetDistancePointPoint(line2.EndPoint, P2))  IntersecPoint2 = line2.StartPoint;
																									else    IntersecPoint2 = line2.EndPoint;
		//Определение лежит ли точка на отрезке
		if (Math.Abs(LI.GetDistancePointPoint(line1.StartPoint, P1) + LI.GetDistancePointPoint(P1, line1.EndPoint) - line1.GetLength())  < 0.00001)  BetweenPoint1 = true; else BetweenPoint1 = false;																							
		if (Math.Abs(LI.GetDistancePointPoint(line2.StartPoint, P2) + LI.GetDistancePointPoint(P2, line2.EndPoint) - line2.GetLength())  < 0.00001)  BetweenPoint2 = true; else BetweenPoint2 = false;
		//Определение маршрута
		if (LenghtIntersec < 0.00001) //случай когда линии пересекаются в пространстве (компланарны)
		{
				if (LI.GetDistancePointPoint(IntersecPoint1, P1)  < 0.00001) 		//точка пересечения совпадает с первой точкой построения
					points = new Point3d[2]{P1, IntersecPoint2};
				else
				{
					if (LI.GetDistancePointPoint(IntersecPoint2, P1)  < 0.00001) 	//точка пересечения совпадает со второй точкой построения
						points = new Point3d[2]{P1, IntersecPoint2};
					else
					{
						if (BetweenPoint1  && BetweenPoint2)  points = new Point3d[1]{P1};								//отрезки непосредственно пересекаются
						if (BetweenPoint1  && !BetweenPoint2) points = new Point3d[2]{P1, IntersecPoint2};					//точка пересечения лежит на отрезке 1
						if (!BetweenPoint1 && BetweenPoint2)  points = new Point3d[2]{IntersecPoint1, P1};					//точка пересечения лежит на отрезке 2
						if (!BetweenPoint1 && !BetweenPoint2) points = new Point3d[3]{IntersecPoint1, P1, IntersecPoint2};	//отрезки пересекаются на продолжении отрезков
					}
				}
		} else	//случай когда линии не компланарны
		{
						if (BetweenPoint1  && BetweenPoint2)  points = new Point3d[2]{P1, P2};									//отрезки непосредственно пересекаются но не компланарны
						if (BetweenPoint1  && !BetweenPoint2) points = new Point3d[3]{P1, P2, IntersecPoint2};					//точка начала пересечения лежит на отрезке 1
						if (!BetweenPoint1 && BetweenPoint2)  points = new Point3d[3]{IntersecPoint1, P1, P2};					//точка начала пересечения лежит на отрезке 2
						if (!BetweenPoint1 && !BetweenPoint2) points = new Point3d[4]{IntersecPoint1, P1, P2, IntersecPoint2};	//отрезки пересекаются на продолжении отрезков
		}
		
		CreateLines2(points, selObj1, selObj2);
		
		return true;
	}   
}
