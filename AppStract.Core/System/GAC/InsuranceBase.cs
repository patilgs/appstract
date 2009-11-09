﻿#region Copyright (C) 2009-2010 Simon Allaeys

/*
    Copyright (C) 2009-2010 Simon Allaeys
 
    This file is part of AppStract

    AppStract is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AppStract is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AppStract.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace AppStract.Core.System.GAC
{
  /// <summary>
  /// Base class for insurances on assembly cache cleanup.
  /// </summary>
  internal abstract class InsuranceBase
  {

    #region Constants

    /// <summary>
    /// Format to use when converting a <see cref="DateTime"/> from and to a <see cref="string"/>.
    /// </summary>
    protected const string _DateTimeFormat = "dd/MM/yyyy HH:mm:ss";

    #endregion

    #region Variables

    private readonly string _insuranceId;
    private readonly string _machineId;
    private readonly DateTime _dateTime;
    private readonly List<AssemblyName> _assemblies;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the string which identifies the current insurance.
    /// </summary>
    public string InsuranceIdentifier
    {
      get { return _insuranceId; }
    }

    /// <summary>
    /// Gets the identifier for the machine for which this insurance is created.
    /// </summary>
    public string MachineId
    {
      get { return _machineId; }
    }

    /// <summary>
    /// Gets the <see cref="DateTime"/> on which the insurance is created.
    /// </summary>
    public DateTime CreationDateTime
    {
      get { return _dateTime; }
    }

    /// <summary>
    /// Gets the assemblies that are insured by the current insurance.
    /// </summary>
    public IEnumerable<AssemblyName> Assemblies
    {
      get { return _assemblies; }
    }

    #endregion

    #region Constructors

    protected InsuranceBase(string insuranceIdentifier, string machineId, DateTime creationDateTime, IEnumerable<AssemblyName> assemblies)
    {
      _insuranceId = insuranceIdentifier;
      _machineId = machineId;
      _dateTime = creationDateTime;
      _assemblies = new List<AssemblyName>(assemblies);
    }

    #endregion

    #region Public Methods

    public virtual void JoinWith(InsuranceBase otherInsurance)
    {
      if (_machineId != otherInsurance._machineId)
        throw new Exception();
      if (_dateTime != otherInsurance._dateTime)
        throw new Exception();
      foreach (var item in otherInsurance._assemblies)
        if (!_assemblies.Contains(item))
          _assemblies.Add(item);
    }

    public virtual bool MatchesWith(InsuranceBase otherInsurance, bool includeAssemblies)
    {
      if (otherInsurance == null
          || _insuranceId != otherInsurance._insuranceId
          || _machineId != otherInsurance._machineId
          || _dateTime != otherInsurance._dateTime)
        return false;
      if (!includeAssemblies)
        return true;
      if (_assemblies.Count != otherInsurance._assemblies.Count)
        return false;
      foreach (var item in otherInsurance._assemblies)
        if (!_assemblies.Contains(item))
          return false;
      return true;
    }

    public override string ToString()
    {
      return "Insurance [" + _dateTime.ToString(_DateTimeFormat) + "] " + _assemblies.Count + " assemblies";
    }

    #endregion

  }
}
