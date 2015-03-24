using System;
using NXOpen;
using NXOpen.BlockStyler;

//------------------------------------------------------------------------------
//Класс построения симметричных точек относительно плоскости
//------------------------------------------------------------------------------
public class Symmetric
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

		public static Vector3 operator-(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		}

		public static Vector3 operator+(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}

		public static Vector3 operator*(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
		}

		public static Vector3 operator*(Vector3 v1, double d)
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

	public Vector3 SymmetricPoint(Vector3 Point0, Vector3 PlanePoint, Vector3 PlaneVector, out double distance)
	{
		Vector3 d = Point0 - PlanePoint;
		distance = -1 * (d ^ PlaneVector) / (PlaneVector ^ PlaneVector);

		return (PlaneVector * (distance * 2)) + Point0;
	}

	public double NXSymmetricPoint(Point3d Point0, Point3d PlanePoint, Vector3d PlaneVector, out Point3d Point1) //точка для симметрии, точка на плоскости, нормаль плоскости, выходная симметричная точка
	{
		double distance;
		Vector3 vPoint0 = new Vector3(Point0.X, Point0.Y, Point0.Z);
		Vector3 vPlanePoint = new Vector3(PlanePoint.X, PlanePoint.Y, PlanePoint.Z);
		Vector3 vPlaneVector = new Vector3(PlaneVector.X, PlaneVector.Y, PlaneVector.Z);
		Vector3 vPoint1 = SymmetricPoint(vPoint0, vPlanePoint, vPlaneVector, out distance);

		Point1 = new Point3d(vPoint1.X, vPoint1.Y, vPoint1.Z);
		return distance;
	}
}

//------------------------------------------------------------------------------
//Represents Block Styler application class
//------------------------------------------------------------------------------

public class UGzkoSegmentSymmetryc
{
	//мои переменные
	Symmetric Sym;
	//class members
    private static Session theSession = null;
	private static UI theUI = null;
	private Part workPart;
	private string theDlxFileName;
	private NXOpen.BlockStyler.BlockDialog theDialog;
	private NXOpen.BlockStyler.UIBlock group0;// Block type: Group
    private NXOpen.BlockStyler.UIBlock plane0;// Block type: Specify Plane
    private NXOpen.BlockStyler.UIBlock separator0;// Block type: Separator
    private NXOpen.BlockStyler.UIBlock selection0;// Block type: Selection

	private TaggedObject SelectedPlane;
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
    public UGzkoSegmentSymmetryc()
	{
		try
		{
			theSession = Session.GetSession();
			workPart = theSession.Parts.Work;
			theUI = UI.GetUI();
			theDlxFileName = "D:\\NXJournal\\NXplugin\\DLX\\UGzkoSegmentSymmetryc.dlx";
			theDialog = theUI.CreateDialog(theDlxFileName);
			theDialog.AddApplyHandler(new NXOpen.BlockStyler.BlockDialog.Apply(apply_cb));
			theDialog.AddOkHandler(new NXOpen.BlockStyler.BlockDialog.Ok(ok_cb));
			theDialog.AddInitializeHandler(new NXOpen.BlockStyler.BlockDialog.Initialize(initialize_cb));
			theDialog.AddFilterHandler(new NXOpen.BlockStyler.BlockDialog.Filter(filter_cb));
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            throw ex;
		}
	}

	//------------------------------- DIALOG LAUNCHING -----------------------------
	//------------------------------------------------------------------------------
    public static void Main()
	{
		UGzkoSegmentSymmetryc theUGzkoSegmentSymmetryc = null;
		try
		{
			theUGzkoSegmentSymmetryc = new UGzkoSegmentSymmetryc();
			// The following method shows the dialog immediately
            theUGzkoSegmentSymmetryc.Show();
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}

		finally
		{
			if(theUGzkoSegmentSymmetryc != null)
				theUGzkoSegmentSymmetryc.Dispose();
			theUGzkoSegmentSymmetryc = null;
		}
	}

	//------------------------------------------------------------------------------
	// This method specifies how a shared image is unloaded from memory
	// within NX. This method gives you the capability to unload an
	// internal NX Open application or user  exit from NX.
	//------------------------------------------------------------------------------
     public static int GetUnloadOption(string arg)
	{
		return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
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
			plane0 = theDialog.TopBlock.FindBlock("plane0");
			separator0 = theDialog.TopBlock.FindBlock("separator0");
			selection0 = theDialog.TopBlock.FindBlock("selection0");
			Sym = new Symmetric();
		}
		catch (Exception ex)
		{
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
				NXOpen.BlockStyler.PropertyList SelectPropsPlane = plane0.GetProperties();
				TaggedObject[] selectedPlane = SelectPropsPlane.GetTaggedObjectVector("SelectedObjects");
				
				NXOpen.BlockStyler.PropertyList SelectLinesProps = selection0.GetProperties(); 
				TaggedObject[] selectedLineS = SelectLinesProps.GetTaggedObjectVector("SelectedObjects");

				foreach (TaggedObject selectedLine in selectedLineS)
					CreateSymmetricPATH(selectedPlane[0], selectedLine);
		}
		catch (Exception ex)
		{
            errorCode = 1;
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}

		return errorCode;
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
    public int filter_cb(NXOpen.BlockStyler.UIBlock selectedBlock, NXOpen.TaggedObject selectedObject)
	{
		int accept = NXOpen.UF.UFConstants.UF_UI_SEL_REJECT;//UF_UI_SEL_ACCEPT;

		if (selectedBlock == selection0)
		{
			try
			{
				if (selectedObject is NXOpen.Routing.LineSegment)
					accept = NXOpen.UF.UFConstants.UF_UI_SEL_ACCEPT;
			}
			catch (Exception ex)
			{
				theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
			}
		}		else
		{
			accept = NXOpen.UF.UFConstants.UF_UI_SEL_ACCEPT;
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
	//Создание симметричного сегмента
	//------------------------------------------------------------------------------
	public bool CreateSymmetricPATH(NXOpen.TaggedObject selPlane, NXOpen.TaggedObject selObj)
	{
		Point3d LineSeg2Point1, LineSeg2Point2; //симметричная прямая

		//получение крайних точек линий
		Line line1 = (Line)selObj;

		//плоскость
		//DatumPlane selDatumPlane = (DatumPlane)selPlane; //не видит фантомную плоскость  (этот пункт потом удалить)
		Plane selDatumPlane = (Plane)selPlane;

		//нахождение симметричных точек
		double distance1 = Sym.NXSymmetricPoint(line1.StartPoint, selDatumPlane.Origin, selDatumPlane.Normal, out LineSeg2Point1);	//при Redo не срабатывает line1.StartPoint  (выдаёт внутреннюю ошибку)
		double distance2 = Sym.NXSymmetricPoint(line1.EndPoint, selDatumPlane.Origin, selDatumPlane.Normal, out LineSeg2Point2);
		
		//построение
		NXOpen.Session.UndoMarkId markId1 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Visible, "Start");

		if ((Math.Abs(distance1) < 0.00001) && (Math.Abs(distance2) < 0.00001)) //случай когда линия лежит на плоскости
		{
			return false;
		}			else
		{
			Point3d[] points = { LineSeg2Point1, LineSeg2Point2 };
			CreateLines2(points, null, null);
		};

		return true;
	}
}
