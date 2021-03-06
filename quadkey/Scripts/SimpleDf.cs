﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;// just for debug.log I think

public enum SdfColType { dffloat, dfstring, dfbool, dfdouble, dfdatetime, dfint, dflong };
public enum SdfConsistencyLevel { none, light, aggressive };

public enum SdfVerbosity { debug, info, warning, error, always };


public class DataError
{
    string cname;
    SdfColType ctype;
    int irow;
    int icol;
    string fieldstring;
    public static Dictionary<string, int> errorCount = new Dictionary<string, int>();
    public static void InitErrorCounts()
    {
        errorCount = new Dictionary<string, int>();
    }
    public static void DumpErrorCounts()
    {
        foreach(var k in errorCount.Keys)
        {
            Debug.LogWarning($"Column {k} has {errorCount[k]} errors");
        }
    }
    public DataError(string cname,SdfColType ctype,int irow,int icol,string fieldstring)
    {
        this.cname = cname;
        this.ctype = ctype;
        this.irow = irow;
        this.icol = icol;
        this.fieldstring = fieldstring;
        if (!errorCount.ContainsKey(cname))
        {
            errorCount[cname] = 0;
        }
        if (ctype== SdfColType.dfdatetime && errorCount[cname]<10)
        {
            Debug.LogWarning($"dfdatetime error row:{irow}  fieldstring:{fieldstring}");
        }
        errorCount[cname]++;
    }
    public override string ToString()
    {
        var s = $"Colunm {cname} of type {ctype} at row:{irow} col:{icol} tried to read {fieldstring}";
        return s;
    }
}

public class SimpleDf
{
    public string name;
    public string comment;
    public System.Globalization.DateTimeStyles dtstyle = System.Globalization.DateTimeStyles.AssumeLocal;
    private Dictionary<string, List<float>> floatcols = null;
    private Dictionary<string, List<string>> stringcols = null;
    private Dictionary<string, List<bool>> boolcols = null;
    private Dictionary<string, List<double>> doubcols = null;
    private Dictionary<string, List<DateTime>> datetimecols = null;
    private Dictionary<string, List<int>> intcols = null;
    private Dictionary<string, List<long>> longcols = null;
    private List<DataError> dataErrors = new List<DataError>();
    private List<string> colnames = null;
    private List<SdfColType> coltypes = null;
    public bool preferFloat = false;
    public Dictionary<string, SdfColType> preferedType = new Dictionary<string, SdfColType>();
    public Dictionary<string, string> preferedFormat = new Dictionary<string, string>();
    public Dictionary<string, (string,string)> preferedSubstitute = new Dictionary<string, (string,string)>();

    public static SdfConsistencyLevel SdfConsistencyLevel = SdfConsistencyLevel.aggressive;
    public SdfVerbosity SdfVerbosity = SdfVerbosity.debug;


    public void Log(SdfVerbosity level,string msg)
    {
        if (level>=SdfVerbosity)
        {
            switch (level)
            {
                case SdfVerbosity.error:
                    Debug.LogError(msg);
                    break;
                case SdfVerbosity.warning:
                    Debug.LogWarning(msg);
                    break;
                default:
                    Debug.Log(msg);
                    break;
            }
        }
    }

    public SimpleDf(string nname="df")
    {
        name = nname;
        InitDf();
    }
    public int DataErrors()
    {
        return dataErrors.Count;
    }
    public string InfoStr()
    {
        var s = $"DataFrame:{name} {comment} ncols:{Ncol()}  nrows:{Nrow()} dataErrors:{DataErrors()}";
        return s;
    }
    public string InfoClassStr()
    {
        var s = "Classes:";
        var ncols = Ncol();
        for (int i = 0; i < ncols; i++)
        {
            var cname = colnames[i];
            var ctype = coltypes[i];
            s += $"{cname}:{ctype}";
            if (i < ncols - 1) s += ",";
        }
        return s;
    }
    public List<string> InfoStatsStr()
    {
        var sl = new List<string>();
        sl.Add($"slx-0:{name} {comment}  cols:{Ncol()}  rows:{Nrow()}");
        sl.Add($"slx-1:");
        sl.Add($"slx-2:");
        sl.Add($"slx-3:");
        sl.Add($"slx-4:");
        var ncols = Ncol();
        var iwid = 20;
        for (int i = 0; i < ncols; i++)
        {
            iwid = Math.Max(iwid, colnames[i].Length);
        }

        for (int i = 0; i < ncols; i++)
        {
            var cname = colnames[i];
            var ctype = coltypes[i];
            sl[1] += $"{AdjustStrWid(cname,iwid)}";
            sl[2] += $"{AdjustStrWid(ctype.ToString(), iwid)}";
            var (sminv, smaxv) = _GetMinMaxValAsString(i, iwid);
            sl[3] += $"{AdjustStrWid(sminv, iwid)}";
            sl[4] += $"{AdjustStrWid(smaxv, iwid)}";
        }
        return sl;
    }
    public void OutputDebugInfoStatStr()
    {
        var sl = InfoStatsStr();
        foreach(var s in sl)
        {
            Debug.Log(s);
        }
    }
    public string InfoInversionsStr()
    { 
        var s = "Inversions:";
        var ncols = Ncol();
        for (int i = 0; i < ncols; i++)
        {
            var cname = colnames[i];
            var ctype = coltypes[i];
            var ninversion = CountInversions(cname);
            s += $"{cname}:{ninversion}";
            if (i < ncols - 1) s += ",";
        }
        return s;
    }
    public void InitDf()
    {
        colnames = new List<string>();
        coltypes = new List<SdfColType>();
        dataErrors = new List<DataError>();

        boolcols = new Dictionary<string, List<bool>>();
        floatcols = new Dictionary<string, List<float>>();
        stringcols = new Dictionary<string, List<string>>();
        doubcols = new Dictionary<string, List<double>>();
        datetimecols = new Dictionary<string, List<DateTime>>();
        intcols = new Dictionary<string, List<int>>();
        longcols = new Dictionary<string, List<long>>();
    }

    DateTime defdatetime = DateTime.UtcNow;
    public int Ncol()
    {
        return floatcols.Count;
    }
    public string GetColname(int i)
    {
        var cn = colnames[i];
        return cn;
    }
    public int Nrow()
    {
        if (floatcols.Count == 0)
        {
            return 0;
        }
        // there is at least one column, so return its length
        var cname = colnames[0];
        var typ = coltypes[0];
        var nrow = 0;
        switch (typ)
        {
            case SdfColType.dfbool:
                nrow = boolcols[cname].Count;
                break;
            case SdfColType.dffloat:
                nrow = floatcols[cname].Count;
                break;
            case SdfColType.dfstring:
                nrow = stringcols[cname].Count;
                break;
            case SdfColType.dfdouble:
                nrow = doubcols[cname].Count;
                break;
            case SdfColType.dfdatetime:
                nrow = datetimecols[cname].Count;
                break;
            case SdfColType.dflong:
                nrow = longcols[cname].Count;
                break;
            case SdfColType.dfint:
                nrow = intcols[cname].Count;
                break;
        }
        return nrow;
    }
    void Assert(bool condition, string name, string message, bool quiet)
    {
        if (!condition)
        {
            var msg = $"Condition {name} failed {message}";
            if (!quiet)
            {
                Debug.LogError(msg);
            }
            throw new ApplicationException(msg);
        }
    }
    public bool CheckConsistency(string caller,bool quiet = false)
    {
        try
        {
            var ncols = Ncol();
            var nrows = Nrow();
            Assert(floatcols.Count == ncols, "floatcols count", "count is wrong", quiet);
            Assert(stringcols.Count == ncols, "stringcols count", "count is wrong", quiet);
            Assert(boolcols.Count == ncols, "boolcols count", "count is wrong", quiet);
            Assert(doubcols.Count == ncols, "doubcols count", "count is wrong", quiet);
            Assert(datetimecols.Count == ncols, "datetime count", "count is wrong", quiet);
            Assert(longcols.Count == ncols, "datetime count", "count is wrong", quiet);

            Assert(colnames.Count == ncols, "colnames count", "count is wrong", quiet);
            Assert(coltypes.Count == ncols, "coltypes count", "count is wrong", quiet);
            for (int i = 0; i < ncols; i++)
            {
                var cname = colnames[i];
                var nflt = 0;
                var nstr = 0;
                var nbool = 0;
                var ndoub = 0;
                var ndt = 0;
                var nlong = 0;
                var nint = 0;
                switch (coltypes[i])
                {
                    case SdfColType.dffloat:
                        nflt = nrows;
                        break;
                    case SdfColType.dfstring:
                        nstr = nrows;
                        break;
                    case SdfColType.dfbool:
                        nbool = nrows;
                        break;
                    case SdfColType.dfdouble:
                        ndoub = nrows;
                        break;
                    case SdfColType.dfdatetime:
                        ndt = nrows;
                        break;
                    case SdfColType.dflong:
                        nlong = nrows;
                        break;
                    case SdfColType.dfint:
                        nint = nrows;
                        break;
                }
                Assert(floatcols[cname].Count == nflt, "floatcol consistency", "wrong number of rows", quiet);
                Assert(stringcols[cname].Count == nstr, "stringcol consistency", "wrong number of rows", quiet);
                Assert(boolcols[cname].Count == nbool, "boolcol consistency", "wrong number of rows", quiet);
                Assert(doubcols[cname].Count == ndoub, "doubcol consistency", "wrong number of rows", quiet);
                Assert(datetimecols[cname].Count == ndt, "datetimecol consistency", "wrong number of rows", quiet);
                Assert(longcols[cname].Count == nlong, "longcol consistency", "wrong number of rows", quiet);
                Assert(intcols[cname].Count == nint, "intcol consistency", "wrong number of rows", quiet);
            }
            if (!quiet)
            {
                Debug.Log($"SimpleDf {name} is consistent");
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return false;
        }
    }

    public float GetFltColRow(int iCol, int jRow)
    {
        var cname = colnames[iCol];
        var vf = 0.0f;
        if (coltypes[iCol] == SdfColType.dfdouble)
        {
            // cast down to float, usually not a problem
            vf = (float) doubcols[cname][jRow];
        }
        else
        {
            vf = floatcols[cname][jRow];
        }
        return vf;
    }
    public bool InRange(int icol,int irow)
    {
        var rv = true;
        if (icol < 0) rv = false;
        if (icol > Ncol()) rv = false;
        if (irow < 0) rv = false;
        if (irow > Nrow()) rv = false;
        return rv;
    }
    public string GetVal(int iCol, int jRow,string defval)
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vs = stringcols[cname][jRow];
        return vs;
    }
    public bool GetVal(int iCol, int jRow,bool defval)
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vb = boolcols[cname][jRow];
        return vb;
    }
    public float GetVal(int iCol, int jRow, float defval)
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vf = floatcols[cname][jRow];
        return vf;
    }
    public double GetVal(int iCol, int jRow,double defval)
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vd = doubcols[cname][jRow];
        return vd;
    }
    public DateTime GetVal(int iCol, int jRow,DateTime defval)
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vdt = datetimecols[cname][jRow];
        return vdt;
    }
    public long GetVal(int iCol, int jRow,long defval)
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vl = longcols[cname][jRow];
        return vl;
    }
    public int GetVal(int iCol, int jRow,int defval   )
    {
        if (!InRange(iCol, jRow)) return defval;
        var cname = colnames[iCol];
        var vi = intcols[cname][jRow];
        return vi;
    }
    public void RecordDigestError(int icol,int irow,string fieldstring)
    {
        var ctype = coltypes[icol];
        var cname = colnames[icol];
        var de = new DataError(cname, ctype, irow, icol, fieldstring);
        dataErrors.Add(de);
    }
    bool Digest(int icol,int irow,string s,bool def)
    {
        var rv = def;
        var ok = bool.TryParse(s, out var res);
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }
    float Digest(int icol, int irow, string s, float def)
    {
        var rv = def;
        var ok = float.TryParse(s, out var res);
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }
    double Digest(int icol, int irow, string s, double def)
    {
        var rv = def;
        var ok = double.TryParse(s, out var res);
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }
    long Digest(int icol, int irow, string s, long def)
    {
        var rv = def;
        var ok = long.TryParse(s, out var res);
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }
    int Digest(int icol, int irow, string s, int def)
    {
        var rv = def;
        var ok = int.TryParse(s, out var res);
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }

    string DigestString(int icol, int irow, string s, string def)
    {
        var rv = def;
        var ok = true;
        var res = s;
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }
 
    DateTime DigestDateTime(int icol, int irow, string s, DateTime def, string prefForm)
    {
        var rv = def;
        s = StripQuotes(s);
        var ok = DateTime.TryParseExact(s, prefForm,null,dtstyle, out var res);
        if (ok)
        {
            rv = res;
        }
        else
        {
            RecordDigestError(icol, irow, s);
        }
        return rv;
    }
  
    SdfColType DetermineColType(string cname,string s)
    {
        // classify the columns according to preference and how they are read
        SdfColType rv = SdfColType.dfstring;
        if (preferedType.ContainsKey(cname))
        {
            rv = preferedType[cname];
            return rv;
        }

        var ok = float.TryParse(s, out var res);
        if (ok)
        {
            if (preferFloat)
            {
                rv = SdfColType.dffloat;
            }
            else
            {
                rv = SdfColType.dfdouble;
            }
        }
        else
        {
            ok = bool.TryParse(s, out var bres);
            if (ok)
            {
                rv = SdfColType.dfbool;
            }
        }
        return rv;
    }
    public static string StripQuotes(string s)
    {
        var rv = s.Trim().Trim('"');// trim leading spaces and quotes
        return rv;
    }
    public void Digest(int icol,int irow, string s)
    {
        var cname = colnames[icol];
        if (preferedSubstitute.ContainsKey(cname))
        {
            var v = preferedSubstitute[cname];
            s = s.Replace(v.Item1, v.Item2);
        }
        switch (coltypes[icol])
        {
            case SdfColType.dfbool:
                {
                    var v = Digest(icol, irow, s, true);
                    boolcols[cname].Add(v);
                }
                break;
            case SdfColType.dffloat:
                {
                    var v = Digest(icol, irow, s, 0.0f);
                    floatcols[cname].Add(v);
                    break;
                }
            case SdfColType.dfdouble:
                {
                    var v = Digest(icol, irow, s, 0.0);
                    doubcols[cname].Add(v);
                    break;
                }
            case SdfColType.dfstring:
                {
                    var v = DigestString(icol, irow, s, "");
                    stringcols[cname].Add(v);
                    break;
                }
            case SdfColType.dfdatetime:
                {
                    var prefForm = "";
                    if (preferedFormat.ContainsKey(cname))
                    {
                        prefForm = preferedFormat[cname];
                    }
                    var v = DigestDateTime(icol, irow, s,defdatetime, prefForm);
                    datetimecols[cname].Add(v);
                    break;
                }
            case SdfColType.dflong:
                {
                    var v = Digest(icol, irow, s, 0L);
                    longcols[cname].Add(v);
                    break;
                }
            case SdfColType.dfint:
                {
                    var v = Digest(icol, irow, s, 0);
                    intcols[cname].Add(v);
                    break;
                }
        }
    }
    public bool ReadCsv(string [] lines)
    {
        DataError.InitErrorCounts();
        var ok = true;
        try
        {
            InitDf();
            var iline = 0;
            var lcolnames = new List<string>();
            int ncommas = 0;
            foreach (var l in lines)
            {
                if (l == "") continue;

                var sar = l.Split(',');
                if (ncommas==0)
                {
                    ncommas = sar.Length;
                }
                else if (ncommas!=sar.Length)
                {
                    Debug.LogWarning($"Inconsistent number of commas first:{ncommas} current:{sar.Length}");
                }
                if (iline == 0)
                {
                    foreach (var s in sar)
                    {
                        var cname = StripQuotes(s);
                        if (cname == "")
                        {
                            cname = "V" + (colnames.Count + 1); // default names from R convention
                        }
                        lcolnames.Add(cname);
                    }
                }
                else
                {
                    var icol = 0;
                    foreach (var s in sar)
                    {
                        var cname = lcolnames[icol];
                        if (iline == 1)
                        {
                            var ctype = DetermineColType(cname, s);
                            _makeNewCol(ctype, cname, "ReadCsv");
                        }
                        Digest(icol,iline, s);
                        icol++;
                    }
                }
                iline++;
            }
        }
        catch (System.Exception ex)
        {
            ok = false;
            Debug.LogError(ex.ToString());
        }
        if (dataErrors.Count>0)
        {
            Debug.LogWarning($"There were {DataErrors()} data errors in {name}");
            Debug.LogWarning($"First error:{dataErrors[0].ToString()}");
            DataError.DumpErrorCounts();
        }
        return ok;
    }
    public void _CopyInColDefs(SimpleDf sdf,string caller   )
    {
        var ncols = sdf.Ncol();
        for (int i = 0; i < ncols; i++)
        {
            var cname = sdf.colnames[i];
            var ctype = sdf.coltypes[i];
            _makeNewCol(ctype, cname,caller);
            //Debug.Log($"In Copy - Adding {cname} of type {ctype} i:{i}");
        }
        foreach(var k in sdf.preferedFormat.Keys)
        {
            preferedFormat[k] = sdf.preferedFormat[k];
        }
    }


    public void _CopyInRows(SimpleDf sdf, List<int> reorderIdx = null, List<bool> boolMask = null)
    {
        // should test for consistency of sdf and ddf columns.
        var nrows = sdf.Nrow();
        var ncols = sdf.Ncol();
        try
        {
            for (int irow = 0; irow < nrows; irow++)
            {
                if (boolMask != null)
                {
                    if (!boolMask[irow]) continue;
                }
                var sirow = irow;
                if (reorderIdx != null)
                {
                    sirow = reorderIdx[irow];
                }
                for (int icol = 0; icol < ncols; icol++)
                {
                    var cname = sdf.colnames[icol];
                    var ctype = sdf.coltypes[icol];
                    //Debug.Log($"Adding {cname} of type {ctype} j:{j} i:{i}");
                    switch (ctype)
                    {
                        case SdfColType.dffloat:
                            {
                                var v = sdf.floatcols[cname][sirow];
                                floatcols[cname].Add(v);
                                break;
                            }
                        case SdfColType.dfstring:
                            {
                                var v = sdf.stringcols[cname][sirow];
                                stringcols[cname].Add(v);
                                break;
                            }
                        case SdfColType.dfbool:
                            {
                                var v = sdf.boolcols[cname][sirow];
                                boolcols[cname].Add(v);
                                break;
                            }
                        case SdfColType.dfdouble:
                            {
                                var v = sdf.doubcols[cname][sirow];
                                doubcols[cname].Add(v);
                                break;
                            }
                        case SdfColType.dfdatetime:
                            {
                                var v = sdf.datetimecols[cname][sirow];
                                datetimecols[cname].Add(v);
                                break;
                            }
                        case SdfColType.dflong:
                            {
                                var v = sdf.longcols[cname][sirow];
                                longcols[cname].Add(v);
                                break;
                            }
                        case SdfColType.dfint:
                            {
                                var v = sdf.intcols[cname][sirow];
                                intcols[cname].Add(v);
                                break;
                            }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception in _CopyInRows:" + ex.ToString());
        }
    }
    public static SimpleDf Copy(SimpleDf sdf, string newname = "ddf",bool quiet=true)
    {
        var ddf = new SimpleDf(newname);
        ddf._CopyInColDefs(sdf, "Copy");
        ddf._CopyInRows(sdf);
        if (SdfConsistencyLevel == SdfConsistencyLevel.aggressive)
        {
            ddf.CheckConsistency("After Copy",quiet: quiet);
        }
        return ddf;
    }
    public static SimpleDf Subset(SimpleDf sdf, string newname = "ddf", List<bool> filter=null, bool quiet = true)
    {
        Debug.Log("Subsetting " + newname);
        var ddf = new SimpleDf(newname);
        ddf._CopyInColDefs(sdf, "Copy");
        ddf._CopyInRows(sdf,reorderIdx:null, boolMask:filter);
        if (SdfConsistencyLevel == SdfConsistencyLevel.aggressive)
        {
            ddf.CheckConsistency("After Subset", quiet: quiet);
        }
        return ddf;
    }
    public bool ReadCsv(string fname,bool quiet=true)
    {
        var ok = true;
        try
        {
            var lines = File.ReadAllLines(fname);
            Debug.Log($"Read {fname} file line count:{lines.Length}" );
            name = fname;
            ReadCsv(lines);
            if (SdfConsistencyLevel >= SdfConsistencyLevel.light)
            {
                CheckConsistency("After ReadCsv", quiet: quiet);
            }
        }
        catch (System.Exception ex)
        {
            ok = false;
            Debug.LogError(ex);
        }
        return ok;
    }
    string _GetFmtString(int icol,int iwid,int iprec)
    {
        var rv = "";
        if (iwid > 0)
        {
            switch (coltypes[icol])
            {
                case SdfColType.dfbool:
                    rv = $"b{iwid}";
                    break;
                case SdfColType.dffloat:
                    rv = $"f{iwid}.{iprec}";
                    break;
                case SdfColType.dfstring:
                    rv = $"b{iwid}";
                    break;
                case SdfColType.dfdouble:
                    rv = $"f{iwid}.{iprec}";
                    break;
                case SdfColType.dfdatetime:
                    rv = $"b{iwid}";
                    break;
                case SdfColType.dflong:
                    rv = $"b{iwid}";
                    break;
                case SdfColType.dfint:
                    rv = $"b{iwid}";
                    break;
            }
        }
        return rv;
    }
    string _GetValAsString(int icol,int irow,int iwid=0,int iprec=1  )
    {
        var rv = "";
        var cname = colnames[icol];
        var fmt = _GetFmtString(icol, iwid, iprec);
        if (preferedFormat.ContainsKey(cname))
        {
            fmt = preferedFormat[cname];
        }
        switch (coltypes[icol])
        {
            case SdfColType.dfbool:
                rv = GetVal(icol, irow, true).ToString();
                break;
            case SdfColType.dffloat:
                rv = GetVal(icol, irow,0.0f).ToString(fmt);
                break;
            case SdfColType.dfstring:
                rv = GetVal(icol, irow,"");
                break;
            case SdfColType.dfdouble:
                rv = GetVal(icol, irow,0.0).ToString(fmt);
                break;
            case SdfColType.dfdatetime:

                //Debug.Log($"{cname} fmt:{fmt}");
                rv = GetVal(icol, irow,defdatetime).ToString(fmt);
                break;
            case SdfColType.dflong:
                rv = GetVal(icol, irow,0L).ToString(fmt);
                break;
            case SdfColType.dfint:
                rv = GetVal(icol, irow,0).ToString(fmt);
                break;
        }
        rv = AdjustStrWid(rv, iwid);
        return rv;
    }
    public string AdjustStrWid(string s,int iwid)
    {
        var rv = s;
        if (iwid > 0 && rv.Length != iwid)
        {
            while (rv.Length < iwid) rv += " ";
            if (rv.Length > iwid)
            {
                int ntoremove = rv.Length - iwid;
                rv.Remove(rv.Length - ntoremove - 1, ntoremove);
            }
        }
        return rv;
    }
    public (string,string) _GetMinMaxValAsString(int icol, int iwid = 0, int iprec = 1)
    {
        var rvMin = "";
        var rvMax = "";
        var cname = colnames[icol];
        var fmt = _GetFmtString(icol, iwid, iprec);
        if (preferedFormat.ContainsKey(cname))
        {
            fmt = preferedFormat[cname];
        }
        switch (coltypes[icol])
        {
            case SdfColType.dfbool:
                {
                    var (minv, _, maxv, _) = GetMinMax<bool>(boolcols[cname]);
                    rvMin = minv.ToString();
                    rvMax = maxv.ToString();
                    break;
                }
            case SdfColType.dffloat:
                {
                    var (minv, _, maxv, _) = GetMinMax<float>(floatcols[cname]);
                    rvMin = minv.ToString();
                    rvMax = maxv.ToString();
                    break;
                }
            case SdfColType.dfstring:
                {
                    var (minv, _, maxv, _) = GetMinMax<string>(stringcols[cname]);
                    rvMin = minv.ToString();
                    rvMax = maxv.ToString();
                    break;
                }
            case SdfColType.dfdouble:
                {
                    var (minv, _, maxv, _) = GetMinMax<double>(doubcols[cname]);
                    rvMin = minv.ToString();
                    rvMax = maxv.ToString();
                    break;
                }
            case SdfColType.dfdatetime:
                {
                    var (minv, _, maxv, _) = GetMinMax<DateTime>(datetimecols[cname]);
                    rvMin = minv.ToString("yyyy-MM-dd HH:mm:ss");
                    rvMax = maxv.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                }
            case SdfColType.dflong:
                {
                    var (minv, _, maxv, _) = GetMinMax<long>(longcols[cname]);
                    rvMin = minv.ToString();
                    rvMax = maxv.ToString();
                    break;
                }
            case SdfColType.dfint:
                {
                    var (minv, _, maxv, _) = GetMinMax<int>(intcols[cname]);
                    rvMin = minv.ToString();
                    rvMax = maxv.ToString();
                    break;
                }
        }
        rvMin = AdjustStrWid(rvMin, iwid);
        rvMax = AdjustStrWid(rvMax, iwid);
        return (rvMin,rvMax);
    }
    string line;
    int dblinnum;
    int dbcolnum;
    string field;
    public bool WriteCsv(string fname)
    {
        var ok = true;
        try
        {
            // Make Header
            var header = "";
            var ncol = Ncol();
            var nrow = Nrow();
            for (int icol = 0; icol < Ncol(); icol++)
            {
                header += GetColname(icol);
                if (icol < ncol-1)
                {
                    header += ",";
                }
            }

            var lines = new List<string>();
            lines.Add(header);
            Debug.Log("Header:" + header);

            for(int irow=0; irow<nrow; irow++)
            {
                line = "";
                dblinnum = irow;
                for(int icol=0; icol<ncol; icol++)
                {
                    dbcolnum = icol;
                    field = _GetValAsString(icol, irow);
                    line += field;
                    if (icol<ncol-1)
                    {
                        line += ",";
                    }
                }
                lines.Add(line);
            }
            File.WriteAllLines(fname,lines);
            Debug.Log($"Wrote {fname} cols:{Ncol()} rows:{Nrow()}");
        }
        catch (System.Exception ex)
        {
            ok = false;
            Debug.LogError($"WriteCsv caught exception creating file {fname} on line:{dblinnum} col:{dbcolnum} field:{field}");
            Debug.LogError(ex);
        }
        return ok;
    }
    public bool FloatTimeFromDateString(string colname, string newcolname)
    {
        var icol = ColIdx(colname);
        if (icol == -1)
        {
            Debug.LogError($"Column {colname} does not exist");
            return false;
        }
        var ctype = coltypes[icol];
        if (ctype != SdfColType.dfdatetime)
        {
            return false;
        }
        var (ok, errmsg) = _makeNewCol(SdfColType.dffloat, newcolname, "FloatTimeFromDateString");
        if (!ok)
        {
            return false;
        }
        var dtcol = datetimecols[colname];
        var tscol = floatcols[newcolname];
        float secs = 0;
        DateTime start = DateTime.MinValue;
        foreach (var t in dtcol)
        {
            if (tscol.Count == 0)
            {
                secs = 0;
                start = t;
            }
            else
            {
                var ts = t - start;
                secs = (float)ts.TotalSeconds;
            }
            tscol.Add(secs);
        }
        if (SdfConsistencyLevel >= SdfConsistencyLevel.light)
        {
            CheckConsistency("After FloatTimeFromDateString");
        }
        return true;
    }

    public int ColIdx(string cname)
    {
        int i = 0;
        foreach (var s in colnames)
        {
            if (s == cname) return i;
            i++;
        }
        return -1;
    }
    public bool ColumnExists(string cname,bool logerror=false,string caller="")
    {
        var rv = ColIdx(cname) >= 0;
        if (!rv && logerror)
        {
            Debug.LogError($"Column {cname} does not exist in {caller}");
        }
        return rv;
    }
    public List<float> GetFloatCol(string colname)
    {
        var icol = ColIdx(colname);
        switch (coltypes[icol])
        {
            case SdfColType.dffloat:
                var rv =  new List<float>(floatcols[colname]);
                return rv;
            default:
                return null;
        }
    }
    public List<string> GetStringCol(string colname)
    {
        var icol = ColIdx(colname);
        switch (coltypes[icol])
        {
            default:
                return null;
            case SdfColType.dfstring:
                var rv = stringcols[colname];
                return rv;
        }
    }
    public List<double> GetDoubleCol(string colname)
    {
        var icol = ColIdx(colname);
        switch (coltypes[icol])
        {
            case SdfColType.dfdouble:
                var rv = new List<double>(doubcols[colname]);
                return rv;
            default:
                return null;
        }
    }
    public List<bool> GetBoolCol(string colname)
    {
        var icol = ColIdx(colname);
        switch (coltypes[icol])
        {
            case SdfColType.dfbool:
                var rv = new List<bool>(boolcols[colname]);
                return rv;
            default:
                return null;
        }
    }
    public List<long> GetLongCol(string colname)
    {
        var icol = ColIdx(colname);
        switch (coltypes[icol])
        {
            case SdfColType.dflong:
                var rv =  new List<long>(longcols[colname]);
                return rv;
            default:
                return null;
        }
    }
    public List<int> GetIntCol(string colname)
    {
        var icol = ColIdx(colname);
        switch (coltypes[icol])
        {
            case SdfColType.dfint:
                var rv = new List<int>(intcols[colname]);
                return rv;
            default:
                return null;
        }
    }

    public (T,int,T,int) GetMinMax<T> (List<T> arr)
    {
        T minv = arr[0];
        T maxv = arr[0];
        var c = Comparer<T>.Default;
        int i = 0;
        int imin = 0;
        int imax = 0;
        foreach ( var v in arr)
        {
            if (c.Compare(v,minv) < 0)
            {
                minv = v;
                imin = i;
            }
            if (c.Compare(v, maxv) > 0)
            {
                maxv = v;
                imax = i;
            }
            i++;
        }
        return (minv,imin, maxv,imax);
    }

    void DeleteOneColumn(string delColName,bool quiet=false)
    {
        var icol = ColIdx(delColName);
        if (icol < 0)
        {
            if (!quiet)
            {
                Debug.LogError($"Can not find {delColName} column in DeleteColumn");
            }
            return;
        }
        coltypes.RemoveAt(icol);
        colnames.RemoveAt(icol);

        boolcols.Remove(delColName);
        floatcols.Remove(delColName);
        stringcols.Remove(delColName);
        doubcols.Remove(delColName);
        datetimecols.Remove(delColName);
        longcols.Remove(delColName);
        intcols.Remove(delColName);
        if (SdfConsistencyLevel == SdfConsistencyLevel.aggressive)
        {
            CheckConsistency("After DeleteOneColumn", quiet: quiet);
        }
    }

    public void RenameColumn(string colname, string colnamenew, bool quiet = false)
    {
        var icol = ColIdx(colname);
        if (icol < 0)
        {
            if (!quiet)
            {
                Debug.LogError($"Can not find {colname} column in RenameColumn");
            }
            return;
        }
        colnames[icol] = colnamenew;

        boolcols[colnamenew] = boolcols[colname];
        floatcols[colnamenew] = floatcols[colname];
        stringcols[colnamenew] = stringcols[colname];
        doubcols[colnamenew] = doubcols[colname];
        stringcols[colnamenew] = stringcols[colname];
        datetimecols[colnamenew] = datetimecols[colname];
        longcols[colnamenew] = longcols[colname];
        intcols[colnamenew] = intcols[colname];

        boolcols.Remove(colname);
        floatcols.Remove(colname);
        stringcols.Remove(colname);
        doubcols.Remove(colname);
        datetimecols.Remove(colname);
        longcols.Remove(colname);
        intcols.Remove(colname);
        if (SdfConsistencyLevel == SdfConsistencyLevel.aggressive)
        {
            CheckConsistency("After RenameColumn", quiet: quiet);
        }
    }

    public void CopyColumn(string colname, string colnamenew, bool quiet = false)
    {
        var icol = ColIdx(colname);
        if (icol < 0)
        {
            if (!quiet)
            {
                Debug.LogError($"Can not find {colname} column in CopyColumn");
            }
            return;
        }
        var ctype = coltypes[icol];
        var (b0, err0) = _makeNewCol(ctype, colnamenew, "CopyColumn");
        if (!b0)
        {
            Debug.LogError(err0);
            return;
        }
        //var icolnew = ColIdx(colnamenew);

        boolcols[colnamenew] = new List<bool>(boolcols[colname]);
        floatcols[colnamenew] = new List<float>(floatcols[colname]);
        stringcols[colnamenew] = new List<string>(stringcols[colname]);
        doubcols[colnamenew] = new List<double>(doubcols[colname]);
        stringcols[colnamenew] = new List<string>(stringcols[colname]);
        datetimecols[colnamenew] = new List<DateTime>(datetimecols[colname]);
        longcols[colnamenew] = new List<long>(longcols[colname]);
        intcols[colnamenew] = new List<int>(intcols[colname]);


        if (SdfConsistencyLevel == SdfConsistencyLevel.aggressive)
        {
            CheckConsistency("After CopyColumn", quiet: quiet);
        }
    }



    public void DeleteColumns(string [] delColNames,bool quiet=false)
    {
        foreach( var col in delColNames)
        {
            DeleteOneColumn(col);
        }
        if (SdfConsistencyLevel >= SdfConsistencyLevel.light)
        {
            CheckConsistency("After DeleteColumns", quiet: quiet);
        }
    }

    public void KeepColumns(string[] keepColNames, bool quiet = false)
    {
        var keepList = new List<string>(keepColNames);
        var delList = new List<string>();
        foreach (var col in colnames)
        {
            if (!keepList.Contains(col))
            {
                delList.Add(col);
            }
        }
        DeleteColumns(delList.ToArray());
    }

    public void DeleteRow(int irow)
    {
        for (int icol=0; icol<Ncol(); icol++ )
        {
            var cname = colnames[icol];
            switch (coltypes[icol])
            {
                case SdfColType.dfbool:
                    boolcols[cname].RemoveAt(irow);
                    break;
                case SdfColType.dffloat:
                    floatcols[cname].RemoveAt(irow);
                    break;
                case SdfColType.dfstring:
                    stringcols[cname].RemoveAt(irow);
                    break;
                case SdfColType.dfdouble   :
                    doubcols[cname].RemoveAt(irow);
                    break;
                case SdfColType.dfdatetime:
                    datetimecols[cname].RemoveAt(irow);
                    break;
                case SdfColType.dflong:
                    longcols[cname].RemoveAt(irow);
                    break;
                case SdfColType.dfint:
                    intcols[cname].RemoveAt(irow);
                    break;
            }
        }
    }
    public void MakeNewFloatCol(string colname,float minval,float maxval)
    {
        var (b0, err0) = _makeNewCol(SdfColType.dffloat, colname,"MakeNewFloatCol");
        if (!b0)
        {
            Debug.LogError(err0);
            return;
        }
        var newcol = floatcols[colname];
        var nrow = Nrow();
        var divor = nrow - 1.0f;
        if (divor == 0) divor = 1;
        for (int irow=0; irow<nrow; irow++)
        {
            var v = minval + (maxval - minval) * irow / divor;
            newcol.Add(minval);
        }
    }
    (bool,string) isFloatCol(string colname)
    {
        if(!floatcols.ContainsKey(colname))
        {
            return (false, $"no column named:\"{colname}\"");
        }
        var icol = ColIdx(colname);
        if (coltypes[icol]!= SdfColType.dffloat)
        {
            return (false, $"colum named:\"{colname}\" is not a float column");
        }
        return (true, "");
    }

    List<bool> GetBoolFilter<T>(List<T> valist,T minval,T maxval)
    {
        var rv = new List<bool>();
        var ntrue = 0;
        foreach(var v in valist)
        {
            var comparer = Comparer<T>.Default;
            var b = (comparer.Compare(minval, v) <0) & (comparer.Compare(v, maxval) < 0);
            if (b) ntrue++;
            rv.Add( b );
        }
        Debug.Log($"GetBoolFilter returned {rv.Count} elements true:{ntrue}");
        return rv;
    }

    public List<bool> GetBoolFilter(string cname,string sminval,string smaxval )
    {
        var rv = new List<bool>();
        var cidx = ColIdx(cname);
        var ctype = coltypes[cidx];
        switch (ctype)
        {
            case SdfColType.dffloat:
                {
                    var mnok = float.TryParse(sminval, out var minval);
                    var mxok = float.TryParse(smaxval, out var maxval);
                    rv = GetBoolFilter<float>(floatcols[cname], minval, maxval );
                    break;
                }
            case SdfColType.dfdouble:
                {
                    var mnok = double.TryParse(sminval, out var minval);
                    var mxok = double.TryParse(smaxval, out var maxval);
                    rv = GetBoolFilter<double>(doubcols[cname], minval, maxval );
                    break;
                }
            case SdfColType.dfint:
                {
                    var mnok = int.TryParse(sminval, out var minval);
                    var mxok = int.TryParse(smaxval, out var maxval);
                    rv = GetBoolFilter<int>(intcols[cname], minval, maxval );
                    break;
                }
            case SdfColType.dflong:
                {
                    var mnok = long.TryParse(sminval, out var minval);
                    var mxok = long.TryParse(smaxval, out var maxval);
                    rv = GetBoolFilter<long>(longcols[cname], minval, maxval);
                    break;
                }
            case SdfColType.dfdatetime:
                {

                    var prefForm = "";
                    if (preferedFormat.ContainsKey(cname))
                    {
                        prefForm = preferedFormat[cname];
                    }
                    var mnok = DateTime.TryParseExact(sminval, prefForm, null, dtstyle, out var minval);
                    var mxok = DateTime.TryParseExact(smaxval, prefForm, null, dtstyle, out var maxval);
                    rv = GetBoolFilter<DateTime>(datetimecols[cname], minval, maxval);
                    break;
                }
            case SdfColType.dfstring    :
                {
                    var minval = sminval;
                    var maxval = smaxval;
                    rv = GetBoolFilter<string>(stringcols[cname], minval, maxval);
                    break;
                }
            case SdfColType.dfbool:
                {
                    var mnok = bool.TryParse(sminval, out var minval);
                    var mxok = bool.TryParse(smaxval, out var maxval);
                    rv = GetBoolFilter<bool>(boolcols[cname], minval, maxval);
                    break;
                }
        }
        return rv;
    }

    public (int,int,float) InterpolateFloat(string colname,float t)
    {
        var maxrow = Nrow();
        var mini = 0;
        var maxi = maxrow-1;
        if (!floatcols.ContainsKey(colname))
        {
            Debug.LogError($"no column named:\"{colname}\"");
            return (mini, maxi, 0);
        }
        if (maxrow<=1)
        {
            return (mini, maxi, 0);
        }
        var fc = floatcols[colname];

        var icnt = 0;
        while (maxi-mini>1)
        {
            var nexti = (mini + maxi) / 2;
            if (fc[nexti]<=t)
            {
                mini = nexti;
            }
            else
            {
                maxi = nexti;
            }
            icnt++;
            if (icnt>20)
            {
                Debug.LogError($"Interpolate max iteration count exceeded:{icnt}");
            }
        }
        if (maxi==mini)
        {
            if (mini==maxrow)
            {
                mini = maxi - 1;

            }
            else
            {
                maxi = mini + 1;
            }
        }
        var denom = fc[maxi] - fc[mini];
        if (denom == 0) denom = 1;
        var lamb = (t - fc[mini]) / denom;
        //Debug.Log($"Interpolate col:{colname} t:{t} mini:{mini}  maxi:{maxi} lamb:{lamb}  icnt:{icnt}");
        return (mini, maxi, lamb);
    }
    public float DeInterploateFloat(string colname, int mini,int maxi,float lamb    )
    {
        if (!floatcols.ContainsKey(colname))
        {
            Debug.LogError($"no column named:\"{colname}\"");
        }
        //Debug.Log($"DeInterpolate col:{colname} mini:{mini}  maxi:{maxi} lamb:{lamb}");
        var fc = floatcols[colname];
        var res = lamb *(fc[maxi] - fc[mini]) + fc[mini];
        return res;
    }
    public double DeInterploateDouble(string colname, int mini, int maxi, float lamb)
    {
        if (!floatcols.ContainsKey(colname))
        {
            Debug.LogError($"no column named:\"{colname}\"");
        }
        //Debug.Log($"DeInterpolate col:{colname} mini:{mini}  maxi:{maxi} lamb:{lamb}");
        var fc = doubcols[colname];
        var res = lamb * (fc[maxi] - fc[mini]) + fc[mini];
        return res;
    }

    (List<T>, List<int>) Sort<T>(List<T> input, bool reverse = false, IComparer<T> comparer = null)
    {
        if (input == null || input.Count == 0)
        {
            return (new List<T>(), new List<int>());
        }
        int[] items = Enumerable.Range(0, input.Count).ToArray();
        T[] keys = input.ToArray();
        if (comparer != null)
        {
            Array.Sort(keys, items, comparer);
        }
        else
        {
            Array.Sort(keys, items);
        }
        var itemslst = new List<int>(items);
        var keylst = new List<T>(keys);
        if (reverse)
        {
            itemslst.Reverse();
            keylst.Reverse();
        }
        return (keylst, itemslst);
    }

    public List<int> GetSortIndex(string cname,bool descend=false  )
    {
        var cidx = ColIdx(cname);
        var ctype = coltypes[cidx];
        var idx = new List<int>();
       switch (ctype)
        {
            case SdfColType.dffloat    :
                (_, idx) = Sort<float>(floatcols[cname],reverse:descend);
                break;
            case SdfColType.dfdouble:
                (_, idx) = Sort<double>(doubcols[cname], reverse:descend);
                break;
            case SdfColType.dfint:
                (_, idx) = Sort<int>(intcols[cname], reverse:descend);
                break;
            case SdfColType.dflong :
                (_, idx) = Sort<long>(longcols[cname], reverse: descend);
                break;
            case SdfColType.dfbool:
                (_, idx) = Sort<bool>(boolcols[cname], reverse:descend);
                break;
            case SdfColType.dfstring:
                (_, idx) = Sort<string>(stringcols[cname], reverse: descend);
                break;
            case SdfColType.dfdatetime:
                (_, idx) = Sort<DateTime>(datetimecols[cname], reverse: descend);
                break;
        }
        return idx;
    }
    public int CountInversions(string cname)
    {
        var rv = 0;
        var idx = GetSortIndex(cname);
        for (int i=0; i<idx.Count-2; i++)
        {
            if(idx[i]>idx[i+1])
            {
                rv++;
            }
        }
        return rv;
    }

    public static SimpleDf SortOnColumn(SimpleDf sdf,string colname,bool descend=false, string newname = "ddf",bool quiet=true)
    {
        var ddf = new SimpleDf(newname);
        if (!sdf.ColumnExists(colname, logerror: true, "SortOnColumn")) return null;

        var idx = sdf.GetSortIndex(colname, descend: descend);
        ddf._CopyInColDefs(sdf, "Copy");
        ddf._CopyInRows(sdf,reorderIdx:idx);
        if (SdfConsistencyLevel >= SdfConsistencyLevel.light)
        {
            ddf.CheckConsistency("After SortOnColumn", quiet: quiet);
        }
        return ddf;
    }

    private (bool,string) _makeNewCol(SdfColType ctype,string colname,string caller,bool quiet=false)
    {
        if (colnames.Contains(colname))
        {
            var msg = $"_makeNewCol: df:{name} already contains column \"{colname}\" - called from {caller}";
            if (!quiet)
            {
                Debug.LogError(msg);
            }
            return (false,msg );
        }
        //Debug.Log($"Adding {colname} to {name}");
        coltypes.Add(ctype);
        colnames.Add(colname);
        floatcols[colname] = new List<float>();
        stringcols[colname] = new List<string>();
        boolcols[colname] = new List<bool>();
        doubcols[colname] = new List<double>();
        datetimecols[colname] = new List<DateTime>();
        longcols[colname] = new List<long>();
        intcols[colname] = new List<int>();
        return (true, "");
    }

    public void LagDoubleCol(string newcolname, string urcolname,int nlag=1, double defval=0)
    {
        _makeNewCol(SdfColType.dfdouble, newcolname,"LagDoubleCol");
        var (b1, err1) = isFloatCol(urcolname);
        if (!b1)
        {
            Debug.LogError(err1);
            return;
        }
        var newcol = doubcols[newcolname];
        var urcol = doubcols[urcolname];
        var nrow = Nrow();
        for (int irow = 0; irow < nrow; irow++)
        {
            var val = defval;
            if (irow>=nlag)
            { 
                val = urcol[irow-nlag];
            }
            newcol.Add(val);
        }
    }

}