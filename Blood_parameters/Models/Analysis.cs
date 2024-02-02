using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class Analysis
{
    public class GeneralBloodTest
    {
        public double hemoglobin_count { get; set; }
        public double red_blood_cells_count { get; set; }
        public double white_blood_cells_count { get; set; }
        public double erythrocyte_sedimentation_rate_count { get; set; }
        public double eosinophil_count { get; set; }
        public double band_neutrophils_coun { get; set; }
        public double segmented_neutrophils_count { get; set; }
        public double lymphocytes_count { get; set; }
        public double monocytes_count { get; set; }
    }

    public class BloodGlucose
    {
        public double BloodGlucoseLevel { get; set; }
    }

    public class BloodCholesterol
    {
        public double BloodCholesterolLevel { get; set; }
    }

    public class BiochemicalBloodAnalysis
    {
        public double TotalBilirubin { get; set; }
        public double DirectBilirubin { get; set; }
        public double IndirectBilirubin { get; set; }
        public double AlanineAminotransferase { get; set; }
        public double AspartateAminotransferase { get; set; }
        public double CreatinineLevel { get; set; }
        public double UrineLevel { get; set; }
    }

    public class BloodPressure
    {
        public double SystolicPressure { get; set; }
        public double DiastolicPressure { get; set; }
        public double PulseRate { get; set; }
    }


    public GeneralBloodTest? generalBloodTest { get; set; }
    public BloodGlucose? bloodGlucose { get; set; }
    public BloodCholesterol? bloodCholesterol { get; set; }
    public BiochemicalBloodAnalysis? biochemicalBloodAnalysis { get; set; }
    public BloodPressure? bloodPressure { get; set; }
    public DateOnly? dateOfCheck { get; set; }
    public int patient_id { get; set; }
}

