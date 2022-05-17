﻿

namespace Models.Soils.Nutrients
{
    using Core;
    using Interfaces;
    using System;
    using APSIM.Shared.Utilities;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using APSIM.Shared.Documentation;

    /// <summary>
    /// This class used for this nutrient encapsulates the nitrogen within a mineral N pool.
    /// Child functions provide information on flows of N from it to other mineral N pools,
    /// or losses from the system.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.ProfileView")]
    [PresenterName("UserInterface.Presenters.ProfilePresenter")]
    [ValidParent(ParentType = typeof(Soil))]
    public class Solute : Model, ISolute
    {
        [Link]
        Soil soil = null;

        /// <summary>Access the soil physical properties.</summary>
        [Link] 
        private IPhysical soilPhysical = null;

        /// <summary>
        /// An enumeration for specifying soil water units
        /// </summary>
        public enum NUnitsEnum
        {
            /// <summary>Volumetric mm/mm</summary>
            ppm,

            /// <summary>kgha</summary>
            kgha
        }

        /// <summary>Default constructor.</summary>
        public Solute() { }

        /// <summary>Default constructor.</summary>
        public Solute(Soil soilModel, string soluteName, double[] value) 
        {
            soil = soilModel;
            kgha = value;
            Name = soluteName;
        }

        /// <summary>Depth strings. Wrapper around Thickness.</summary>
        [Description("Depth")]
        [Units("cm")]
        [JsonIgnore]
        public string[] Depth
        {
            get
            {
                return SoilUtilities.ToDepthStrings(Thickness);
            }
            set
            {
                Thickness = SoilUtilities.ToThickness(value);
            }
        }

        /// <summary>Thickness</summary>
        [Summary]
        [Units("mm")]
        public double[] Thickness { get; set; }

        /// <summary>Nitrate NO3.</summary>
        [Description("Initial values")]
        [Summary]
        public double[] InitialValues { get; set; }

        /// <summary>Units of the Initial values.</summary>
        public NUnitsEnum InitialValuesUnits { get; set; }

        /// <summary>Solute amount (kg/ha)</summary>
        [JsonIgnore]
        public double[] kgha { get; set; }

        /// <summary>Solute amount (ppm)</summary>
        public double[] ppm { get { return SoilUtilities.kgha2ppm(soilPhysical.Thickness, soilPhysical.BD, kgha); } }

        /// <summary>Performs the initial checks and setup</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("StartOfSimulation")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            Reset();
        }

        /// <summary>
        /// Set solute to initialisation state
        /// </summary>
        public void Reset()
        {
            if (InitialValues == null)
                kgha = new double[Thickness.Length];
            else if (InitialValuesUnits == NUnitsEnum.kgha)
                kgha = ReflectionUtilities.Clone(InitialValues) as double[];
            else
                kgha = SoilUtilities.ppm2kgha(Thickness, soilPhysical.BD, InitialValues);
        }
        /// <summary>Setter for kgha.</summary>
        /// <param name="callingModelType">Type of calling model.</param>
        /// <param name="value">New values.</param>
        public void SetKgHa(SoluteSetterType callingModelType, double[] value)
        {
            for (int i = 0; i < value.Length; i++)
                kgha[i] = value[i];
        }

        /// <summary>Setter for kgha delta.</summary>
        /// <param name="callingModelType">Type of calling model</param>
        /// <param name="delta">New delta values</param>
        public void AddKgHaDelta(SoluteSetterType callingModelType, double[] delta)
        {
            for (int i = 0; i < delta.Length; i++)
                kgha[i] += delta[i];
        }

        /// <summary>
        /// Document the model.
        /// </summary>
        public override IEnumerable<ITag> Document()
        {
            foreach (ITag tag in DocumentChildren<Memo>())
                yield return tag;

            foreach (ITag tag in GetModelDescription())
                yield return tag;
        }
    }
}
