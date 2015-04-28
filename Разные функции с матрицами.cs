	//ћатрица поворота вида и матрица поворота дуги независимы друг от друга, и отсчитываютс€ только от начальной системы координат
	//=> угол поворота матрицы дуги относительно матрицы вида равен (матрица вида)*(обратна€ матрица дуги)
	//http://matrixcalc.org/	- онлайн расчЄт матриц
	//http://blog2k.ru/archives/3197	- блок схема дл€ определени€ поворота
	//http://en.wikipedia.org/wiki/Rotation_matrix	- касательно матриц поворота (с примерами)
	//http://en.wikipedia.org/wiki/Axis%E2%80%93angle_representation#Log_map_from_SO.283.29_to_so.283.29	- доп.материал
	public static double GetMatrixDirection(NXOpen.Matrix3x3 viewM, NXOpen.Matrix3x3 arcM)		//Ќаправление и угол (дл€ дуг и элипсов)
	{
		/*ќдин из вариантов можно использовать встроенные функции
		double [] uf_viewM = convert_Matrix3x3_to_UFMatrix(viewM);
		double [] uf_arcM  = convert_Matrix3x3_to_UFMatrix(arcM);
		double [] uf_arcM_T = new double[9]; 
		double [] uf_M = new double[9]; 
		ufs.Mtx3.Transpose(uf_arcM, uf_arcM_T);
		ufs.Mtx3.Multiply(uf_viewM, uf_arcM_T, uf_M);*/
		
		//считаем что nx нас не обманывает и определители матриц равны 1
		//транспонируем матрицу дуги (обратное вращение)
		NXOpen.Matrix3x3 arcMT = arcM;
		arcMT.Xy = arcM.Yx;		arcMT.Yx = arcM.Xy;
		arcMT.Xz = arcM.Zx;		arcMT.Zx = arcM.Xz;
		arcMT.Yz = arcM.Zy;		arcMT.Zy = arcM.Yz;
		//
		//PrintSTR(arcMT.ToString());
		//”множение матрицы вида на транспонированную, получаем матрицу поворота вид->дуга
		NXOpen.Matrix3x3 M = new NXOpen.Matrix3x3();
		M.Xx = viewM.Xx * arcMT.Xx + viewM.Xy * arcMT.Yx + viewM.Xz * arcMT.Zx;
		M.Xy = viewM.Xx * arcMT.Xy + viewM.Xy * arcMT.Yy + viewM.Xz * arcMT.Zy;
		M.Xz = viewM.Xx * arcMT.Xz + viewM.Xy * arcMT.Yz + viewM.Xz * arcMT.Zz;

		M.Yx = viewM.Yx * arcMT.Xx + viewM.Yy * arcMT.Yx + viewM.Yz * arcMT.Zx;
		M.Yy = viewM.Yx * arcMT.Xy + viewM.Yy * arcMT.Yy + viewM.Yz * arcMT.Zy;
		M.Yz = viewM.Yx * arcMT.Xz + viewM.Yy * arcMT.Yz + viewM.Yz * arcMT.Zz;

		M.Zx = viewM.Zx * arcMT.Xx + viewM.Zy * arcMT.Yx + viewM.Zz * arcMT.Zx;
		M.Zy = viewM.Zx * arcMT.Xy + viewM.Zy * arcMT.Yy + viewM.Zz * arcMT.Zy;
		M.Zz = viewM.Zx * arcMT.Xz + viewM.Zy * arcMT.Yz + viewM.Zz * arcMT.Zz;
		//
		//PrintSTR(M.ToString());
		//¬ычисление "следа" матрицы  (Trace(M))
		double traceM = M.Xx + M.Yy + M.Zz;
		//”гол поворота
		double ang = Math.Acos((traceM - 1)*0.5);
		//PrintSTR(traceM.ToString());
		//PrintSTR(ang.ToString());
		//упрощение расчЄта
		double preCalc = 1/(2*Math.Sin(ang));
		//вектор поворота
		NXOpen.VectorArithmetic.Vector3 vAng = new NXOpen.VectorArithmetic.Vector3();
		vAng.x = preCalc * (M.Yz - M.Zy);
		vAng.y = preCalc * (M.Zx - M.Xz);
		vAng.z = preCalc * (M.Xy - M.Yx);
		//вектор оси Z вида
		NXOpen.VectorArithmetic.Vector3 vView = new NXOpen.VectorArithmetic.Vector3(viewM.Zx, viewM.Zy, viewM.Zz);
		//вектор оси Z дуги
		NXOpen.VectorArithmetic.Vector3 vArc = new NXOpen.VectorArithmetic.Vector3(arcM.Zx, arcM.Zy, arcM.Zz);
		
		//”гол между ос€ми Z  по формуле    скал€рное произведение векторов разделить на умноженные длины векторов
		double angZ = Math.Acos((vArc.x*vView.x + vArc.y*vView.y +vArc.z*vView.z)/(vArc.LengthSqr()*vView.LengthSqr()));
		
		if (angZ > 0.000001) ang = -ang;
		return ang*180/Math.PI;
	}
	
	public static double GetMatrixDirection3(UFSession ufs, NXOpen.Matrix3x3 viewM, NXOpen.Matrix3x3 arcM)		//Ќаправление и угол (дл€ дуг и элипсов)
	{
		//считаем что nx нас не обманывает и определители матриц равны 1
		//считаем что вектор Z вида и вектор Z дуги 	коллинеарны
		//=> угол между осью X (или Y) будет углом поворота, а за направление следует прин€ть один из векторов Z
		
		double [] vecX_view = {viewM.Xx, viewM.Xy, viewM.Xz};
		double [] vecX_arc  = {arcM.Xx,  arcM.Xy,  arcM.Xz};
		double [] vecZ_view = {viewM.Zx, viewM.Zy, viewM.Zz};
		double [] vecZ_arc  = {arcM.Zx,  arcM.Zy,  arcM.Zz};
		double ang, angZ;
		ufs.Vec3.AngleBetween(vecX_view, vecX_arc, vecZ_view, out ang);
		ufs.Vec3.AngleBetween(vecZ_view, vecZ_arc, vecX_view, out angZ);
		if (angZ > 0.000001) ang = -ang;
		return ang*180/Math.PI;
	}
	
	
		public static double convertANG_ABStoCSYS(UFSession ufs, double ang, NXOpen.Matrix3x3 viewM, NXOpen.Matrix3x3 arcM)		//Ќаправление и угол (дл€ дуг и элипсов)
	{
		double [] uf_viewM = convert_Matrix3x3_to_UFMatrix(viewM);
		double [] uf_arcM  = convert_Matrix3x3_to_UFMatrix(arcM);
		//double [] uf_arcM_T = new double[9]; 
		//double [] uf_M = new double[9]; 
		//ufs.Mtx3.Transpose(uf_arcM, uf_arcM_T);
		//ufs.Mtx3.Multiply(uf_viewM, uf_arcM_T, uf_M);
		
		double [] model_pt = {Math.Cos(ang), Math.Sin(ang), 0};
		double [] result = {0,0,0};
		ufs.Mtx3.VecMultiplyT(model_pt, uf_arcM, result);
		ufs.Mtx3.VecMultiply(result, uf_viewM, result);
		//PrintSTR("x = " + result[0] + "\t\t Acos=" + Math.Acos(result[0])*180/Math.PI+ "\t Asin=" + Math.Asin(result[0])*180/Math.PI);
		//PrintSTR("y = " + result[1] + "\t\t Acos=" + Math.Acos(result[1])*180/Math.PI+ "\t Asin=" + Math.Asin(result[1])*180/Math.PI);
		//PrintSTR("z = " + result[2] + "\t\t Acos=" + Math.Acos(result[2])*180/Math.PI+ "\t Asin=" + Math.Asin(result[2])*180/Math.PI);
		int znak = 1;
		//if ((Math.Abs(Math.Acos(result[0]) - Math.Asin(result[1])) > 0.000001) && (Math.Abs(Math.Acos(result[0]) + Math.Asin(result[1]) - 180) < 0.000001)) znak = -1;			//в обратную сторону по тригонометрии
		if (result[1] < 0) znak = -1;
		return znak * Math.Acos(result[0]);
	}