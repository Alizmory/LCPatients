using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LCPatients.Models;

namespace LCPatients.Controllers
{
    public class LCMedicationController : Controller
    {
        private readonly LCPatientsContext _context;

        public LCMedicationController(LCPatientsContext context)
        {
            _context = context;
        }

        // GET: LCMedication
        public async Task<IActionResult> Index(string MedicationTypeId)
        {
            if (MedicationTypeId != null)
            {
                // Store in Cookies
                Response.Cookies.Append("MedicationTypeId", MedicationTypeId);
            }
            else if (Request.Query["MedicationTypeId"].Any())
            {
                // Store in Cookies
                MedicationTypeId = Request.Query["MedicationTypeId"].ToString();
                Response.Cookies.Append("MedicationTypeId", MedicationTypeId);
            }
            else if (Request.Cookies["MedicationTypeId"] != null)
            {
                // Retrive the Value from Cookies
                MedicationTypeId = Request.Cookies["MedicationTypeId"].ToString();
            }
            else
            {
                TempData["message"] = "Please Select a Medication Type to see its Medications";
                return RedirectToAction("Index",
                    "LCMedicationType");
            }
            var medicationType = _context.MedicationTypes.Where(a => a.MedicationTypeId == Convert.ToInt32(MedicationTypeId)).FirstOrDefault();
            ViewData["medicationName"] = medicationType.Name;
            var patientsContext = _context.Medications.
                 Include(m => m.ConcentrationCodeNavigation).
                 Include(m => m.DispensingCodeNavigation).
                 Include(m => m.MedicationType).
                 Where(a => a.MedicationTypeId == Convert.ToInt32(MedicationTypeId)).
                 OrderBy(a => a.Name).
                 ThenBy(a => a.Concentration);
            return View(await patientsContext.ToListAsync());
        }

        // GET: LCMedication/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Medications == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            ViewData["medicationName"] = medication.Name;
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // GET: LCMedication/Create
        public IActionResult Create()
        {
            string MedicationTypeId = string.Empty;
            if (Request.Cookies["MedicationTypeId"] != null)
            {
                //Retrive value
                MedicationTypeId = Request.Cookies["MedicationTypeId"].ToString();
            }
            var medicationType = _context.MedicationTypes.Where(a => a.MedicationTypeId == Convert.ToInt32(MedicationTypeId)).FirstOrDefault();
            ViewData["medicationName"] = medicationType.Name;

            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnits.OrderBy(a => a.ConcentrationCode), "ConcentrationCode", "ConcentrationCode");
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnits.OrderBy(a => a.DispensingCode), "DispensingCode", "DispensingCode");
            //ViewData["MedicationTypeId"] = new SelectList(_context.MedicationTypes, "MedicationTypeId", "MedicationTypeId");
            return View();
        }

        // POST: LCMedication/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Din,Name,Image,MedicationTypeId,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            string MedicationTypeId = string.Empty;
            if (Request.Cookies["MedicationTypeId"] != null)
            {
                // Retrive value from the Cookies
                MedicationTypeId = Request.Cookies["MedicationTypeId"].ToString();
            }
            medication.MedicationTypeId = Convert.ToInt32(MedicationTypeId);
            var isDuplicate = _context.Medications.Where(a => a.Name == medication.Name && a.Concentration == medication.Concentration && a.ConcentrationCode == medication.ConcentrationCode);
            if (isDuplicate.Any())
            {
                ModelState.AddModelError("", "Dupicate Entry");
            }

            if (ModelState.IsValid)
            {
                _context.Add(medication);
                await _context.SaveChangesAsync();
                TempData["Success"] = "New Record Added";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnits, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnits, "DispensingCode", "DispensingCode", medication.DispensingCode);
            return View(medication);
        }

        // GET: LCMedication/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Medications == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications.FindAsync(id);
            ViewData["medicationName"] = medication.Name;
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnits, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnits, "DispensingCode", "DispensingCode", medication.DispensingCode);
            //ViewData["MedicationTypeId"] = new SelectList(_context.MedicationTypes, "MedicationTypeId", "MedicationTypeId", medication.MedicationTypeId);
            return View(medication);
        }

        // POST: LCMedication/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Din,Name,Image,MedicationTypeId,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            if (id != medication.Din)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Din))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnits, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnits, "DispensingCode", "DispensingCode", medication.DispensingCode);
            //ViewData["MedicationTypeId"] = new SelectList(_context.MedicationTypes, "MedicationTypeId", "MedicationTypeId", medication.MedicationTypeId);
            return View(medication);
        }

        // GET: LCMedication/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Medications == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            ViewData["medicationName"] = medication.Name;
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // POST: LCMedication/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Medications == null)
            {
                return Problem("Entity set 'LCPatientsContext.Medications'  is null.");
            }
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                _context.Medications.Remove(medication);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicationExists(string id)
        {
          return _context.Medications.Any(e => e.Din == id);
        }
    }
}
