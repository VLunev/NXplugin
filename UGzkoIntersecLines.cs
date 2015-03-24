using System;
using System.IO;
using NXOpen;
using NXOpen.BlockStyler;

//------------------------------------------------------------------------------
//Класс пересечения линий 3D
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
						
			public static double operator^(Vector3 v1, Vector3 v2)	//dot   суммирование всех перемноженных членов(сделано для удобства, можно вывести в отдельную функцию)
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

			//if (D < EPS) 
			if (Math.Abs(b) > EPS)
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
			//if (!result) return -1;
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
public class UGzko2
{
	//Мои переменные
	LinesIntersection LI;
	//class members
    private static Session theSession = null;
	private Part workPart;
	private static UI theUI = null;
	private string theDlxFileName;
	private NXOpen.BlockStyler.BlockDialog theDialog;
	private NXOpen.BlockStyler.UIBlock group0;// Block type: Group
    private NXOpen.BlockStyler.UIBlock selection0;// Block type: Selection


	//------------------------------------------------------------------------------
	//Bit Option for Property: SnapPointTypesEnabled
	//------------------------------------------------------------------------------
    public static readonly int SnapPointTypesEnabled_UserDefined = (1 << 0);
	public static readonly int SnapPointTypesEnabled_Inferred = (1 << 1);
	public static readonly int SnapPointTypesEnabled_ScreenPosition = (1 << 2);
	public static readonly int SnapPointTypesEnabled_EndPoint = (1 << 3);
	public static readonly int SnapPointTypesEnabled_MidPoint = (1 << 4);
	public static readonly int SnapPointTypesEnabled_ControlPoint = (1 << 5);
	public static readonly int SnapPointTypesEnabled_Intersection = (1 << 6);
	public static readonly int SnapPointTypesEnabled_ArcCenter = (1 << 7);
	public static readonly int SnapPointTypesEnabled_QuadrantPoint = (1 << 8);
	public static readonly int SnapPointTypesEnabled_ExistingPoint = (1 << 9);
	public static readonly int SnapPointTypesEnabled_PointonCurve = (1 <<10);
	public static readonly int SnapPointTypesEnabled_PointonSurface = (1 <<11);
	public static readonly int SnapPointTypesEnabled_PointConstructor = (1 <<12);
	public static readonly int SnapPointTypesEnabled_TwocurveIntersection = (1 <<13);
	public static readonly int SnapPointTypesEnabled_TangentPoint = (1 <<14);
	public static readonly int SnapPointTypesEnabled_Poles = (1 <<15);
	public static readonly int SnapPointTypesEnabled_BoundedGridPoint = (1 <<16);
	//------------------------------------------------------------------------------
	//Bit Option for Property: SnapPointTypesOnByDefault
	//------------------------------------------------------------------------------
    public static readonly int SnapPointTypesOnByDefault_EndPoint = (1 << 3);
	public static readonly int SnapPointTypesOnByDefault_MidPoint = (1 << 4);
	public static readonly int SnapPointTypesOnByDefault_ControlPoint = (1 << 5);
	public static readonly int SnapPointTypesOnByDefault_Intersection = (1 << 6);
	public static readonly int SnapPointTypesOnByDefault_ArcCenter = (1 << 7);
	public static readonly int SnapPointTypesOnByDefault_QuadrantPoint = (1 << 8);
	public static readonly int SnapPointTypesOnByDefault_ExistingPoint = (1 << 9);
	public static readonly int SnapPointTypesOnByDefault_PointonCurve = (1 <<10);
	public static readonly int SnapPointTypesOnByDefault_PointonSurface = (1 <<11);
	public static readonly int SnapPointTypesOnByDefault_PointConstructor = (1 <<12);
	public static readonly int SnapPointTypesOnByDefault_BoundedGridPoint = (1 <<16);

	//------------------------------------------------------------------------------
	//Constructor for NX Styler class
	//------------------------------------------------------------------------------
    public UGzko2()
	{
		try
		{
			theSession = Session.GetSession();
			workPart = theSession.Parts.Work;
			theUI = UI.GetUI();
			theDlxFileName = "D:\\NXJournal\\NXplugin\\DLX\\UGzkoIntersecLines.dlx";
			theDialog = theUI.CreateDialog(theDlxFileName);
			theDialog.AddApplyHandler(new NXOpen.BlockStyler.BlockDialog.Apply(apply_cb));
			theDialog.AddOkHandler(new NXOpen.BlockStyler.BlockDialog.Ok(ok_cb));
			theDialog.AddUpdateHandler(new NXOpen.BlockStyler.BlockDialog.Update(update_cb));
			theDialog.AddInitializeHandler(new NXOpen.BlockStyler.BlockDialog.Initialize(initialize_cb));
			theDialog.AddDialogShownHandler(new NXOpen.BlockStyler.BlockDialog.DialogShown(dialogShown_cb));

			theDialog.AddFilterHandler(new NXOpen.BlockStyler.BlockDialog.Filter(filter_cb));
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            throw ex;
		}
	}

	//------------------------------- DIALOG LAUNCHING ---------------------------------
	//
	//    Before invoking this application one needs to open any part/empty part in NX
	//    because of the behavior of the blocks.
	//
	//    Make sure the dlx file is in one of the following locations:
	//        1.) From where NX session is launched
	//        2.) $UGII_USER_DIR/application
	//        3.) For released applications, using UGII_CUSTOM_DIRECTORY_FILE is highly
	//            recommended. This variable is set to a full directory path to a file 
	//            containing a list of root directories for all custom applications.
	//            e.g., UGII_CUSTOM_DIRECTORY_FILE=$UGII_ROOT_DIR\menus\custom_dirs.dat
	//
	//    You can create the dialog using one of the following way:
	//
	//    1. Journal Replay
	//
	//        1) Replay this file through Tool->Journal->Play Menu.
	//
	//    2. USER EXIT
	//
	//        1) Create the Shared Library -- Refer "Block UI Styler programmer's guide"
	//        2) Invoke the Shared Library through File->Execute->NX Open menu.
	//
	//------------------------------------------------------------------------------
    public static void Main()
	{
		UGzko2 theUGzko2 = null;
		try
		{
			theUGzko2 = new UGzko2();
			// The following method shows the dialog immediately
            theUGzko2.Show();
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}

		finally
		{
			if(theUGzko2 != null)
				theUGzko2.Dispose();
			theUGzko2 = null;
		}
	}

	//------------------------------------------------------------------------------
	// This method specifies how a shared image is unloaded from memory
	// within NX. This method gives you the capability to unload an
	// internal NX Open application or user  exit from NX. Specify any
	// one of the three constants as a return value to determine the type
	// of unload to perform:
	//
	//
	//    Immediately : unload the library as soon as the automation program has completed
	//    Explicitly  : unload the library from the "Unload Shared Image" dialog
	//    AtTermination : unload the library when the NX session terminates
	//
	//
	// NOTE:  A program which associates NX Open applications with the menubar
	// MUST NOT use this option since it will UNLOAD your NX Open application image
	// from the menubar.
	//------------------------------------------------------------------------------
     public static int GetUnloadOption(string arg)
	{
		//return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
		return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
		// return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
	}

	//------------------------------------------------------------------------------
	// Following method cleanup any housekeeping chores that may be needed.
	// This method is automatically called by NX.
	//------------------------------------------------------------------------------
    public static int UnloadLibrary(string arg)
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

		return 0;
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
			group0 = theDialog.TopBlock.FindBlock("group0");
			selection0 = theDialog.TopBlock.FindBlock("selection0");
			LI = new LinesIntersection();
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
	}

	//------------------------------------------------------------------------------
	//Callback Name: dialogShown_cb
	//This callback is executed just before the dialog launch. Thus any value set 
	//here will take precedence and dialog will be launched showing that value. 
	//------------------------------------------------------------------------------
    public void dialogShown_cb()
	{
		try
		{
			//---- Enter your callback code here -----
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
	}

	//------------------------------------------------------------------------------
	//Callback Name: apply_cb
	//------------------------------------------------------------------------------
    public int apply_cb()
	{
		int errorCode = 0;
		try
		{
			//---- Enter your callback code here -----
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            errorCode = 1;
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}

		return errorCode;
	}

	//------------------------------------------------------------------------------
	//Callback Name: update_cb			Срабатывание при клике по объекту
	//------------------------------------------------------------------------------
    public int update_cb(NXOpen.BlockStyler.UIBlock block)
	{
		try
		{
			if(block == selection0)
			{
							
				NXOpen.BlockStyler.PropertyList SelectLinesProps = selection0.GetProperties(); 
				TaggedObject[] selectedLineS = SelectLinesProps.GetTaggedObjectVector("SelectedObjects");

				if (selectedLineS.Length == 2) //2 выделенных объекта
				{
					CreatePATH(selectedLineS[0], selectedLineS[1]);
					SelectLinesProps.SetTaggedObjectVector("SelectedObjects", new TaggedObject[0]);
				}	
			/*
				Selection selectionMan = theUI.SelectionManager;
				if (selectionMan.GetNumSelectedObjects() == 2) //2 выделенных объекта
				{
					CreatePATH(selectionMan.GetSelectedTaggedObject(0), selectionMan.GetSelectedTaggedObject(1));
					
					NXOpen.BlockStyler.PropertyList SelectProps = block.GetProperties(); 
					TaggedObject[] linseg = new TaggedObject[0];
					SelectProps.SetTaggedObjectVector("SelectedObjects", linseg);
				}*/
			}
		}
		catch (Exception ex)
		{
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}

		return 0;
	}

	//------------------------------------------------------------------------------
	//Callback Name: ok_cb
	//------------------------------------------------------------------------------
    public int ok_cb()
	{
		int errorCode = 0;
		try
		{
			errorCode = apply_cb();
			//---- Enter your callback code here -----
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            errorCode = 1;
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}

		return errorCode;
	}

	//------------------------------------------------------------------------------
	//Callback Name: filter_cb
	//------------------------------------------------------------------------------
    public int filter_cb(NXOpen.BlockStyler.UIBlock selectedBlock, NXOpen.TaggedObject selectedObject)
	{
		int accept = NXOpen.UF.UFConstants.UF_UI_SEL_REJECT;//UF_UI_SEL_ACCEPT;
		try
		{
			if (selectedObject is NXOpen.Routing.LineSegment)
				accept = NXOpen.UF.UFConstants.UF_UI_SEL_ACCEPT;
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
