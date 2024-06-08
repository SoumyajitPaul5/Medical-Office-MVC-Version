using MedicalOffice.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MedicalOffice.Data
{
    public static class MOInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            MedicalOfficeContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<MedicalOfficeContext>();

            try
            {
                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();
                //context.Database.Migrate();

                //To randomly generate data
                Random random = new Random();

                //Provinces, Cities 
                if (!context.Provinces.Any())
                {
                    var provinces = new List<Province>
                    {
                        new Province { ID = "ON", Name = "Ontario"},
                        new Province { ID = "PE", Name = "Prince Edward Island"},
                        new Province { ID = "NB", Name = "New Brunswick"},
                        new Province { ID = "BC", Name = "British Columbia"},
                        new Province { ID = "NL", Name = "Newfoundland and Labrador"},
                        new Province { ID = "SK", Name = "Saskatchewan"},
                        new Province { ID = "NS", Name = "Nova Scotia"},
                        new Province { ID = "MB", Name = "Manitoba"},
                        new Province { ID = "QC", Name = "Quebec"},
                        new Province { ID = "YT", Name = "Yukon"},
                        new Province { ID = "NU", Name = "Nunavut"},
                        new Province { ID = "NT", Name = "Northwest Territories"},
                        new Province { ID = "AB", Name = "Alberta"}
                    };
                    context.Provinces.AddRange(provinces);
                    context.SaveChanges();
                }
                if (!context.Cities.Any())
                {
                    var cities = new List<City>
                    {
                        new City { Name = "Toronto", ProvinceID="ON" },
                        new City { Name = "Halifax", ProvinceID="NS" },
                        new City { Name = "Calgary", ProvinceID="AB" },
                        new City { Name = "Winnipeg", ProvinceID="MB", },
                        new City { Name = "Stratford", ProvinceID="ON" },
                        new City { Name = "St. Catharines", ProvinceID="ON" },
                        new City { Name = "Stratford", ProvinceID="PE" },
                        new City { Name = "Ancaster", ProvinceID="ON" },
                        new City { Name = "Vancouver", ProvinceID="BC" },
                    };
                    context.Cities.AddRange(cities);
                    context.SaveChanges();
                }

                // Looking for any Doctors.  Since we can't have patients without Doctors.
                if (!context.Doctors.Any())
                {
                    context.Doctors.AddRange(
                    new Doctor
                    {
                        FirstName = "Gregory",
                        MiddleName = "A",
                        LastName = "House"
                    },
                    new Doctor
                    {
                        FirstName = "Doogie",
                        MiddleName = "R",
                        LastName = "Houser"
                    },
                    new Doctor
                    {
                        FirstName = "Charles",
                        LastName = "Xavier"
                    });
                    context.SaveChanges();
                }

                //Adding some Medical Trials
                if (!context.MedicalTrials.Any())
                {
                    context.MedicalTrials.AddRange(
                     new MedicalTrial
                     {
                         TrialName = "UOT - Leukemia Treatment"
                     }, new MedicalTrial
                     {
                         TrialName = "HyGIeaCare Center -  Microbiome Analysis of Constipated Versus Non-constipation Patients"
                     }, new MedicalTrial
                     {
                         TrialName = "Sunnybrook -  Trial of BNT162b2 versus mRNA-1273 COVID-19 Vaccine Boosters in Chronic Kidney Disease and Dialysis Patients With Poor Humoral Response following COVID-19 Vaccination"
                     }, new MedicalTrial
                     {
                         TrialName = "Altimmune -  Evaluate the Effect of Position and Duration on the Safety and Immunogenicity of Intranasal AdCOVID Administration"
                     }, new MedicalTrial
                     {
                         TrialName = "TUK - Hair Loss Treatment"
                     });
                    context.SaveChanges();
                }

                // Seeding Patients if there aren't any.
                if (!context.Patients.Any())
                {
                    context.Patients.AddRange(
                    new Patient
                    {
                        FirstName = "Fred",
                        MiddleName = "Reginald",
                        LastName = "Flintstone",
                        OHIP = "1231231234",
                        DOB = DateTime.Parse("1955-09-01"),
                        ExpYrVisits = 6,
                        Phone = "9055551212",
                        EMail = "fflintstone@outlook.com",
                        DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Gregory" && d.LastName == "House").ID,
                        MedicalTrialID = context.MedicalTrials.FirstOrDefault(d => d.TrialName.Contains("UOT")).ID
                    },
                    new Patient
                    {
                        FirstName = "Wilma",
                        MiddleName = "Jane",
                        LastName = "Flintstone",
                        OHIP = "1321321324",
                        DOB = DateTime.Parse("1964-04-23"),
                        ExpYrVisits = 2,
                        Phone = "9055551212",
                        EMail = "wflintstone@outlook.com",
                        MedicalTrialID = context.MedicalTrials.FirstOrDefault(d => d.TrialName.Contains("UOT")).ID,
                        DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Gregory" && d.LastName == "House").ID
                    },
                    new Patient
                    {
                        FirstName = "Barney",
                        LastName = "Rubble",
                        OHIP = "3213213214",
                        DOB = DateTime.Parse("1964-02-22"),
                        ExpYrVisits = 2,
                        Phone = "9055551213",
                        EMail = "brubble@outlook.com",
                        DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Doogie" && d.LastName == "Houser").ID
                    },
                    new Patient
                    {
                        FirstName = "Jane",
                        MiddleName = "Samantha",
                        LastName = "Doe",
                        OHIP = "4124124123",
                        ExpYrVisits = 2,
                        Phone = "9055551234",
                        EMail = "jdoe@outlook.com",
                        DoctorID = context.Doctors.FirstOrDefault(d => d.FirstName == "Charles" && d.LastName == "Xavier").ID
                    });
                    context.SaveChanges();
                }

                //Adding more Doctors
                if (context.Doctors.Count() < 4)
                {
                    string[] firstNames = new string[] { "Woodstock", "Violet", "Charlie", "Lucy", "Linus", "Franklin", "Marcie", "Schroeder" };
                    string[] lastNames = new string[] { "Hightower", "Broomspun", "Jones", "Bloggs", "Brown", "Smith", "Daniel" };

                    //Looping through names and adding more
                    foreach (string lastName in lastNames)
                    {
                        foreach (string firstname in firstNames)
                        {
                            //Construct some details
                            Doctor a = new Doctor()
                            {
                                FirstName = firstname,
                                LastName = lastName,
                                //Take second character of the last name and make it the middle name
                                MiddleName = lastName[1].ToString().ToUpper(),
                            };
                            context.Doctors.Add(a);
                        }
                    }
                    context.SaveChanges();
                }

                int[] doctorIDs = context.Doctors.Select(a => a.ID).ToArray();
                int doctorIDCount = doctorIDs.Length;
                int[] medicalTrialIDs = context.MedicalTrials.Select(a => a.ID).ToArray();
                int medicalTrialIDCount = medicalTrialIDs.Length;

                string[] specialties = new string[] { "Abdominal Radiology", "Addiction Psychiatry", "Adolescent Medicine Pediatrics", "Cardiothoracic Anesthesiology", "Adult Reconstructive Orthopaedics", "Advanced Heart Failure ", "Allergy & Immunology ", "Anesthesiology ", "Biochemical Genetics", "Blood Banking ", "Cardiothoracic Radiology", "Cardiovascular Disease Internal Medicine", "Chemical Pathology", "Child & Adolescent Psychiatry", "Child Abuse Pediatrics", "Child Neurology", "Clinical & Laboratory Immunology", "Clinical Cardiac Electrophysiology", "Clinical Neurophysiology Neurology", "Colon & Rectal Surgery ", "Congenital Cardiac Surgery", "Craniofacial Surgery", "Critical Care Medicine", "Cytopathology ", "Dermatology ", "Dermatopathology ", "Family Medicine ", "Family Practice", "Female Pelvic Medicine", "Foot & Ankle Orthopaedics", "Forensic Pathology", "Forensic Psychiatry ", "Hand Surgery", "Hematology Pathology", "Oncology ", "Infectious Disease", "Internal Medicine ", "Interventional Cardiology", "Neonatal-Perinatal Medicine", "Nephrology Internal Medicine", "Neurological Surgery ", "Neurology ", "Neuromuscular Medicine", "Neuropathology Pathology", "Nuclear Medicine ", "Nuclear Radiology", "Obstetric Anesthesiology", "Obstetrics & Gynecology ", "Ophthalmic Plastic", "Ophthalmology ", "Orthopaedic Sports Medicine", "Orthopaedic Surgery ", "Otolaryngology ", "Otology", "Pediatrics ", "Plastic Surgery ", "Preventive Medicine ", "Radiation Oncology ", "Rheumatology", "Vascular & Interventional Radiology", "Vascular Surgery", "Integrated Thoracic Surgery", "Transplant Hepatology", "Urology" };
                if (!context.Specialties.Any())
                {
                    foreach (string s in specialties)
                    {
                        Specialty sp = new Specialty
                        {
                            SpecialtyName = s
                        };
                        context.Specialties.Add(sp);
                    }
                    context.SaveChanges();
                }
                int[] specialtyIDs = context.Specialties.Select(s => s.ID).ToArray();
                int specialtyIDCount = specialtyIDs.Length;

                if (!context.DoctorSpecialties.Any())
                {
                    //i loops through the primary keys of the Doctors
                    //j is just a counter so we add some Specialities to a Doctor
                    //k lets us step through all Specialties so we can make sure each gets used
                    int k = 0;//Start with the first Specialty
                    foreach (int i in doctorIDs)
                    {
                        int howMany = random.Next(1, 10);
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= specialtyIDCount) ? 0 : k;
                            DoctorSpecialty ds = new DoctorSpecialty()
                            {
                                DoctorID = i,
                                SpecialtyID = specialtyIDs[k]
                            };
                            k++;
                            context.DoctorSpecialties.Add(ds);
                        }
                        context.SaveChanges();
                    }
                }



                //Adding more Patients.
                if (context.Patients.Count() < 5)
                {
                    string[] firstNames = new string[] { "Lyric", "Antoinette", "Kendal", "Vivian", "Ruth", "Jamison", "Emilia", "Natalee", "Yadiel", "Jakayla", "Lukas", "Moses", "Kyler", "Karla", "Chanel", "Tyler", "Camilla", "Quintin", "Braden", "Clarence" };
                    string[] lastNames = new string[] { "Watts", "Randall", "Arias", "Weber", "Stone", "Carlson", "Robles", "Frederick", "Parker", "Morris", "Soto", "Bruce", "Orozco", "Boyer", "Burns", "Cobb", "Blankenship", "Houston", "Estes", "Atkins", "Miranda", "Zuniga", "Ward", "Mayo", "Costa", "Reeves", "Anthony", "Cook", "Krueger", "Crane", "Watts", "Little", "Henderson", "Bishop" };
                    int firstNameCount = firstNames.Length;


                    DateTime startDOB = DateTime.Today;
                    int counter = 1; 

                    foreach (string lastName in lastNames)
                    {
                        HashSet<string> selectedFirstNames = new HashSet<string>();
                        while (selectedFirstNames.Count() < 5)
                        {
                            selectedFirstNames.Add(firstNames[random.Next(firstNameCount)]);
                        }

                        foreach (string firstName in selectedFirstNames)
                        {
                            Patient patient = new Patient()
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                MiddleName = lastName[1].ToString().ToUpper(),
                                OHIP = random.Next(2, 9).ToString() + random.Next(213214131, 989898989).ToString(),
                                EMail = (firstName.Substring(0, 2) + lastName + random.Next(11, 111).ToString() + "@outlook.com").ToLower(),
                                Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString(),
                                ExpYrVisits = (byte)random.Next(2, 12),
                                DOB = startDOB.AddDays(-random.Next(60, 34675)),
                                DoctorID = doctorIDs[random.Next(doctorIDCount)]
                            };
                            if (counter % 3 == 0)
                            {
                                patient.MedicalTrialID = medicalTrialIDs[random.Next(medicalTrialIDCount)];
                            }
                            counter++;
                            context.Patients.Add(patient);
                            try
                            {
                              
                                context.SaveChanges();
                            }
                            catch (Exception)
                            {
                                
                            }
                        }
                    }
                    
                    string cmd = "DELETE FROM Doctors WHERE NOT EXISTS(SELECT 1 FROM Patients WHERE Doctors.Id = Patients.DoctorID)";
                    context.Database.ExecuteSqlRaw(cmd);
                }

                string[] AppointmentReasons = new string[] { "Illness", "Accident", "Mental State", "Annual Checkup", "COVID-19", "Work Injury" };
                if (!context.AppointmentReasons.Any())
                {
                    foreach (string s in AppointmentReasons)
                    {
                        AppointmentReason ar = new AppointmentReason
                        {
                            ReasonName = s
                        };
                        context.AppointmentReasons.Add(ar);
                    }
                    context.SaveChanges();
                }

                string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                int[] AppointmentReasonIDs = context.AppointmentReasons.Select(s => s.ID).ToArray();
                int AppointmentReasonIDCount = AppointmentReasonIDs.Length;

                int[] patientIDs = context.Patients.Select(d => d.ID).ToArray();
                int patientIDCount = patientIDs.Length;

                doctorIDs = context.Doctors.Select(a => a.ID).ToArray();
                doctorIDCount = doctorIDs.Length;


                if (!context.Appointments.Any())
                {
                    foreach (int i in patientIDs)
                    {
                        //i loops through the primary keys of the Patients
                        //j is just a counter so we add some Appointments to a Patient
                        //k lets us step through all AppointmentReasons so we can make sure each gets used
                        int k = 0;//Start with the first AppointmentReason
                        int howMany = random.Next(1, AppointmentReasonIDCount);
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= AppointmentReasonIDCount) ? 0 : k;
                            Appointment a = new Appointment()
                            {
                                PatientID = i,
                                AppointmentReasonID = AppointmentReasonIDs[k],
                                StartTime = DateTime.Today.AddDays(-1 * random.Next(120)),
                                Notes = baconNotes[random.Next(5)],
                                DoctorID = doctorIDs[random.Next(doctorIDCount)]
                            };
                            a.StartTime = a.StartTime.AddHours(random.Next(8, 17));
                            a.EndTime = a.StartTime + new TimeSpan(0, random.Next(1, 12) * 10, 0);

                            if (k % 2 == 0) a.ExtraFee = 0d;
                            k++;
                            context.Appointments.Add(a);
                        }
                        context.SaveChanges();
                    }
                }

                //Conditions
                if (!context.Conditions.Any())
                {
                    string[] conditions = new string[] { "Asthma", "Cancer", "Cardiac disease", "Diabetes", "Hypertension", "Seizure disorder", "Circulation problems", "Bleeding disorder", "Thyroid condition", "Liver Disease", "Measles", "Mumps" };

                    foreach (string condition in conditions)
                    {
                        Condition c = new Condition
                        {
                            ConditionName = condition
                        };
                        context.Conditions.Add(c);
                    }
                    context.SaveChanges();
                }

                //PatientConditions
                if (!context.PatientConditions.Any())
                {
                    context.PatientConditions.AddRange(
                        new PatientCondition
                        {
                            ConditionID = context.Conditions.FirstOrDefault(c => c.ConditionName == "Cancer").ID,
                            PatientID = context.Patients.FirstOrDefault(p => p.LastName == "Flintstone" && p.FirstName == "Fred").ID
                        },
                        new PatientCondition
                        {
                            ConditionID = context.Conditions.FirstOrDefault(c => c.ConditionName == "Cardiac disease").ID,
                            PatientID = context.Patients.FirstOrDefault(p => p.LastName == "Flintstone" && p.FirstName == "Fred").ID
                        },
                        new PatientCondition
                        {
                            ConditionID = context.Conditions.FirstOrDefault(c => c.ConditionName == "Diabetes").ID,
                            PatientID = context.Patients.FirstOrDefault(p => p.LastName == "Flintstone" && p.FirstName == "Wilma").ID
                        });
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
