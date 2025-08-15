using System;
using System.Collections.Generic;

namespace HealthSystemAppNS
{
    public class Repository<T>
    {
        private readonly List<T> items = new List<T>();

        public void Add(T item)
        {
            items.Add(item);
        }

        public List<T> GetAll()
        {
            return items;
        }

        public T? GetById(Func<T, bool> predicate)
        {
            foreach (var it in items)
            {
                if (predicate(it)) return it;
            }
            return default;
        }

        public bool Remove(Func<T, bool> predicate)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (predicate(items[i]))
                {
                    items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }

    public class Patient
    {
        public int Id;
        public string Name;
        public int Age;
        public string Gender;

        public Patient(int id, string name, int age, string gender)
        {
            Id = id;
            Name = name;
            Age = age;
            Gender = gender;
        }
    }

    public class Prescription
    {
        public int Id;
        public int PatientId;
        public string MedicationName;
        public DateTime DateIssued;

        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id;
            PatientId = patientId;
            MedicationName = medicationName;
            DateIssued = dateIssued;
        }
    }

    public class HealthSystemApp
    {
        private readonly Repository<Patient> _patientRepo = new Repository<Patient>();
        private readonly Repository<Prescription> _prescriptionRepo = new Repository<Prescription>();
        private readonly Dictionary<int, List<Prescription>> _prescriptionMap = new Dictionary<int, List<Prescription>>();

        public void SeedData()
        {
            _patientRepo.Add(new Patient(1, "Ama Mensah", 28, "Female"));
            _patientRepo.Add(new Patient(2, "Kojo Asare", 35, "Male"));
            _patientRepo.Add(new Patient(3, "Yaw Boateng", 42, "Male"));

            _prescriptionRepo.Add(new Prescription(1, 1, "Amoxicillin 500 mg", DateTime.Today.AddDays(-7)));
            _prescriptionRepo.Add(new Prescription(2, 1, "Ibuprofen 200 mg", DateTime.Today.AddDays(-3)));
            _prescriptionRepo.Add(new Prescription(3, 2, "Paracetamol 500 mg", DateTime.Today.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(4, 2, "Loratadine 10 mg", DateTime.Today.AddDays(-1)));
            _prescriptionRepo.Add(new Prescription(5, 3, "Metformin 500 mg", DateTime.Today));
        }

        public void BuildPrescriptionMap()
        {
            _prescriptionMap.Clear();
            foreach (var p in _prescriptionRepo.GetAll())
            {
                if (!_prescriptionMap.ContainsKey(p.PatientId))
                {
                    _prescriptionMap[p.PatientId] = new List<Prescription>();
                }
                _prescriptionMap[p.PatientId].Add(p);
            }
        }

        public void PrintAllPatients()
        {
            Console.WriteLine("Patients:");
            foreach (var p in _patientRepo.GetAll())
            {
                Console.WriteLine($"Id: {p.Id}, Name: {p.Name}, Age: {p.Age}, Gender: {p.Gender}");
            }
        }

        public List<Prescription> GetPrescriptionsByPatientId(int patientId)
        {
            if (_prescriptionMap.TryGetValue(patientId, out var list))
            {
                return list;
            }
            return new List<Prescription>();
        }

        public void PrintPrescriptionsForPatient(int id)
        {
            var patient = _patientRepo.GetById(p => p.Id == id);
            if (patient == null)
            {
                Console.WriteLine("Patient not found");
                return;
            }

            var meds = GetPrescriptionsByPatientId(id);
            Console.WriteLine($"Prescriptions for {patient.Name} (Id {id}):");
            if (meds.Count == 0)
            {
                Console.WriteLine("None");
                return;
            }

            foreach (var rx in meds)
            {
                Console.WriteLine($"Rx Id: {rx.Id}, Drug: {rx.MedicationName}, Date: {rx.DateIssued:d}");
            }
        }

        public void Run()
        {
            SeedData();
            BuildPrescriptionMap();
            PrintAllPatients();
            Console.WriteLine();
            PrintPrescriptionsForPatient(2);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new HealthSystemApp();
            app.Run();
        }
    }
}
