﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLib.Param;
using BaseLib.Parse;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Load{
	public class UnstructuredTxtUpload : IMatrixUpload{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.raw; } }
		public string Name { get { return "Raw upload"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 10; } }
		public string Description { get { return "Load all lines from a text file and put them into a single text column or split them into multiple text columns."; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(ref string errString){
			return
				new Parameters(new Parameter[]{
					new FileParam("File"){
						Filter = "Text (Tab delimited) (*.txt)|*.txt|CSV (Comma delimited) (*.csv)|*.csv|All files (*.*)|*.*",
						Help = "Please specify here the name of the file to be uploaded including its full path."
					},
					new BoolWithSubParams("Split into columns", false)
					{SubParamsTrue = new Parameters(new SingleChoiceParam("Separator"){Values = new[]{"Tab", "Comma"}})}
				});
		}

		public void LoadData(IMatrixData mdata, Parameters parameters, ProcessInfo processInfo){
			string filename = parameters.GetFileParam("File").Value;
			BoolWithSubParams bsp = parameters.GetBoolWithSubParams("Split into columns");
			bool split = bsp.Value;
			if (split){
				bool csv = bsp.GetSubParameters().GetSingleChoiceParam("Separator").Value == 1;
				LoadSplit(mdata, filename, csv);
			} else{
				LoadNoSplit(mdata, filename);
			}
		}

		private static void LoadNoSplit(IMatrixData mdata, string filename){
			List<string> lines = new List<string>();
			StreamReader reader = new StreamReader(filename);
			string line;
			while ((line = reader.ReadLine()) != null){
				lines.Add(line);
			}
			reader.Close();
			mdata.SetData("", "", new List<string>(), new List<string>(), new float[lines.Count,0], new bool[lines.Count,0],
				new float[lines.Count,0], "", true, new List<string>(new[]{"All data"}),
				new List<string>(new[]{"Complete file in one text column."}), new List<string[]>(new[]{lines.ToArray()}),
				new List<string>(), new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(),
				new List<double[]>(), new List<string>(), new List<string>(), new List<double[][]>(), new List<string>(),
				new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>());
		}

		private static void LoadSplit(IMatrixData mdata, string filename, bool csv){
			char separator = csv ? ',' : '\t';
			string[] colNames = TabSep.GetColumnNames(filename, 0, PerseusUtils.commentPrefix,
				PerseusUtils.commentPrefixExceptions, null, separator);
			string[][] cols = TabSep.GetColumns(colNames, filename, 0, PerseusUtils.commentPrefix,
				PerseusUtils.commentPrefixExceptions, separator);
			int nrows = TabSep.GetRowCount(filename);
			mdata.SetData("", "", new List<string>(), new List<string>(), new float[nrows,0], new bool[nrows,0],
				new float[nrows,0], "", true, new List<string>(colNames), new List<string>(colNames), new List<string[]>(cols),
				new List<string>(), new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(),
				new List<double[]>(), new List<string>(), new List<string>(), new List<double[][]>(), new List<string>(),
				new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>());
		}
	}
}