using BillMgmt.Data;
using BillMgmt.Models.Bill;
using BillMgmt.Models.ViewModels;
using BillMgmt.Models.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BillMgmt.Controllers
{
    public class BillController : Controller
    {
        #region Fields / DbContext

        /// EF6 DbContext used for all Bill module database operations.
        private readonly AppDbContext _db = new AppDbContext();

        #endregion


        /// Index screen:
        /// - Lists bills with Customer/Store names
        /// - Shows totals + total quantity (sum of item quantities)
        public async Task<ActionResult> Index()
        {
            #region Query Bills (AsNoTracking for performance)

            var list = await _db.Bills
                .AsNoTracking()
                .Include(b => b.Customer)
                .Include(b => b.Store)
                .OrderByDescending(b => b.BillId)
                .Select(b => new BillIndexVm
                {
                    BillId = b.BillId,
                    BillDate = b.BillDate,
                    CustomerName = b.Customer.CustomerName,
                    StoreName = b.Store.StoreName,
                    TotalAmount = b.TotalAmount,

                    // Total Qty = sum of item qty
                    TotalQty = b.Items.Sum(i => (decimal?)i.Qty) ?? 0m
                })
                .ToListAsync();

            #endregion

            return View(list);
        }



        /// Create screen:
        /// - Loads dropdowns (Customers/Stores)
        /// - Returns empty BillVm
        public async Task<ActionResult> Create()
        {
            var vm = new BillVm();

            #region Load Dropdowns

            vm.Customers = await _db.Customers
                .AsNoTracking()
                .OrderBy(x => x.CustomerName)
                .Select(x => new SelectListItem
                {
                    Value = x.CustomerId.ToString(),
                    Text = x.CustomerName
                })
                .ToListAsync();

            vm.Stores = await _db.Stores
                .AsNoTracking()
                .OrderBy(x => x.StoreName)
                .Select(x => new SelectListItem
                {
                    Value = x.StoreId.ToString(),
                    Text = x.StoreName
                })
                .ToListAsync();

            #endregion

            return View(vm);
        }



        /// Edit screen:
        /// - Loads bill + items + product names
        /// - Loads dropdowns (Customers/Stores)
        /// - Fills BillVm
        public async Task<ActionResult> Edit(int id)
        {
            #region Load Bill with Items + Product (read-only)

            var bill = await _db.Bills
                .AsNoTracking()
                .Include(b => b.Items.Select(i => i.Product))
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
                return HttpNotFound();

            #endregion

            #region Build ViewModel

            var vm = new BillVm
            {
                BillId = bill.BillId,
                CustomerId = bill.CustomerId,
                StoreId = bill.StoreId,
                TotalAmount = bill.TotalAmount,

                Items = bill.Items.Select(i => new BillItemVm
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    Price = i.Price,
                    Qty = i.Qty
                }).ToList()
            };

            #endregion

            #region Load Dropdowns

            vm.Customers = await _db.Customers
                .AsNoTracking()
                .OrderBy(x => x.CustomerName)
                .Select(x => new SelectListItem
                {
                    Value = x.CustomerId.ToString(),
                    Text = x.CustomerName
                })
                .ToListAsync();

            vm.Stores = await _db.Stores
                .AsNoTracking()
                .OrderBy(x => x.StoreName)
                .Select(x => new SelectListItem
                {
                    Value = x.StoreId.ToString(),
                    Text = x.StoreName
                })
                .ToListAsync();

            #endregion

            return View(vm);
        }



        /// Save bill (Create or Edit)
        /// Best Practices implemented:
        /// - Server-side validations (even if you have JS)
        /// - Re-check store/product/stock at save time
        /// - Calculates totals from items (not from UI)
        /// - Edit uses Replace Items approach (remove + add)
        /// - Transaction to keep Bill + Items consistent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(BillVm vm, string postAction)
        {
            #region Basic Required Validation (Customer/Store)

            if (!vm.CustomerId.HasValue)
                ModelState.AddModelError("CustomerId", "اختر العميل");

            if (!vm.StoreId.HasValue)
                ModelState.AddModelError("StoreId", "اختر المخزن");

            #endregion

            #region Items Validation (Required + Clean)

            vm.Items = vm.Items ?? new List<BillItemVm>();

            if (!vm.Items.Any())
                ModelState.AddModelError("Items", "يجب إضافة عنصر واحد على الأقل.");

            #endregion

            #region Business Validation (Products / Qty / Price / Duplicates / Store Stock)

            if (vm.StoreId.HasValue && vm.Items.Any())
            {
                // 1) prevent duplicate product ids in same bill
                var duplicateProductIds = vm.Items
                    .GroupBy(i => i.ProductId)
                    .Where(g => g.Key > 0 && g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateProductIds.Any())
                    ModelState.AddModelError("Items", "يوجد منتجات مكررة داخل الفاتورة. احذف التكرار ثم حاول مرة أخرى.");

                // 2) validate each item basic values
                foreach (var it in vm.Items)
                {
                    if (it.ProductId <= 0)
                        ModelState.AddModelError("Items", "يوجد بند بدون منتج.");

                    if (it.Qty <= 0)
                        ModelState.AddModelError("Items", "يوجد بند بكمية غير صحيحة (يجب أن تكون أكبر من صفر).");

                    if (it.Price < 0)
                        ModelState.AddModelError("Items", "يوجد بند بسعر غير صحيح (لا يمكن أن يكون سالب).");
                }

                // 3) validate product exists in store + stock available (server-side)
                if (ModelState.IsValid)
                {
                    var storeId = vm.StoreId.Value;

                    var productIds = vm.Items.Select(x => x.ProductId).Distinct().ToList();

                    var storeProducts = await _db.StoreProducts
                        .AsNoTracking()
                        .Where(sp => sp.StoreId == storeId && productIds.Contains(sp.ProductId))
                        .Select(sp => new { sp.ProductId, sp.StockQty, sp.Price })
                        .ToListAsync();

                    var missing = productIds.Except(storeProducts.Select(s => s.ProductId)).ToList();
                    if (missing.Any())
                        ModelState.AddModelError("Items", "يوجد منتج غير موجود داخل هذا المخزن.");

                    // UPDATED PART (StockQty == 0 + Qty > StockQty)
                    foreach (var it in vm.Items)
                    {
                        var sp = storeProducts.FirstOrDefault(x => x.ProductId == it.ProductId);
                        if (sp == null) continue;

                        if (sp.StockQty <= 0)
                        {
                            ModelState.AddModelError("Items",
                                $"المنتج ({it.ProductId}) غير متوفر بالمخزن (الرصيد = 0).");
                        }
                        else if (it.Qty > sp.StockQty)
                        {
                            ModelState.AddModelError("Items",
                                $"الكمية المطلوبة للمنتج ({it.ProductId}) أكبر من الرصيد الحالي بالمخزن.");
                        }
                    }
                }
            }

            #endregion

            #region If Invalid -> Reload Dropdowns + Return Same View

            if (!ModelState.IsValid)
            {
                vm.Customers = await _db.Customers
                    .AsNoTracking()
                    .OrderBy(x => x.CustomerName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.CustomerId.ToString(),
                        Text = x.CustomerName
                    })
                    .ToListAsync();

                vm.Stores = await _db.Stores
                    .AsNoTracking()
                    .OrderBy(x => x.StoreName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.StoreId.ToString(),
                        Text = x.StoreName
                    })
                    .ToListAsync();

                return vm.BillId.HasValue ? View("Edit", vm) : View("Create", vm);
            }

            #endregion

            #region Calculate Totals (Server-Side)

            var total = vm.Items.Sum(x => x.Price * x.Qty);

            #endregion

            #region Save Using Transaction (Bill + Items)

            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    Bill bill;

                    if (vm.BillId.HasValue)
                    {
                        bill = await _db.Bills
                            .Include(b => b.Items)
                            .FirstOrDefaultAsync(b => b.BillId == vm.BillId.Value);

                        if (bill == null)
                            return HttpNotFound();

                        bill.CustomerId = vm.CustomerId.Value;
                        bill.StoreId = vm.StoreId.Value;
                        bill.TotalAmount = total;

                        _db.BillItems.RemoveRange(bill.Items);
                        bill.Items.Clear();

                        foreach (var it in vm.Items)
                        {
                            bill.Items.Add(new BillItem
                            {
                                ProductId = it.ProductId,
                                Price = it.Price,
                                Qty = it.Qty
                            });
                        }
                    }
                    else
                    {
                        bill = new Bill
                        {
                            CustomerId = vm.CustomerId.Value,
                            StoreId = vm.StoreId.Value,
                            BillDate = DateTime.Now,
                            TotalAmount = total
                        };

                        foreach (var it in vm.Items)
                        {
                            bill.Items.Add(new BillItem
                            {
                                ProductId = it.ProductId,
                                Price = it.Price,
                                Qty = it.Qty
                            });
                        }

                        _db.Bills.Add(bill);
                    }

                    await _db.SaveChangesAsync();
                    tx.Commit();

                    TempData["Success"] = "تم حفظ الفاتورة بنجاح ✅";
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            #endregion

            #region Redirect After Save

            if (string.Equals(postAction, "createNew", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Create");

            return RedirectToAction("Index");

            #endregion
        }




        /// Delete confirmation screen:
        /// GET: /Bill/Delete/5
        /// - Shows bill basic info before deleting
        public async Task<ActionResult> Delete(int id)
        {
            #region Load Bill (AsNoTracking)

            var bill = await _db.Bills
                .AsNoTracking()
                .Include(b => b.Customer)
                .Include(b => b.Store)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
                return HttpNotFound();

            #endregion

            return View(bill); // Create Views/Bill/Delete.cshtml
        }

        /// Executes delete:
        /// POST: /Bill/Delete/5
        /// - Deletes BillItems first then Bill (safe with FK constraints)
        /// - Transaction to ensure consistent delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    #region Load Bill (tracking) with items

                    var bill = await _db.Bills
                        .Include(b => b.Items)
                        .FirstOrDefaultAsync(b => b.BillId == id);

                    if (bill == null)
                        return HttpNotFound();

                    #endregion

                    #region Delete children then parent

                    _db.BillItems.RemoveRange(bill.Items);
                    _db.Bills.Remove(bill);

                    await _db.SaveChangesAsync();

                    #endregion

                    tx.Commit();
                    TempData["Success"] = "تم حذف الفاتورة بنجاح ✅";
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            return RedirectToAction("Index");
        }



        /// Returns products available in a store (for Product dropdown).
        /// GET: /Bill/GetProductsByStore?storeId=1
        public async Task<JsonResult> GetProductsByStore(int storeId)
        {
            var products = await _db.StoreProducts
                .AsNoTracking()
                .Where(sp => sp.StoreId == storeId)
                .Select(sp => new
                {
                    id = sp.ProductId,
                    name = sp.Product.ProductName
                })
                .OrderBy(x => x.name)
                .ToListAsync();

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        /// Returns stock qty and price for selected product in selected store.
        /// GET: /Bill/GetProductInfo?storeId=1&productId=2
        public async Task<JsonResult> GetProductInfo(int storeId, int productId)
        {
            var data = await _db.StoreProducts
                .AsNoTracking()
                .Where(x => x.StoreId == storeId && x.ProductId == productId)
                .Select(x => new { x.StockQty, x.Price })
                .FirstOrDefaultAsync();

            if (data == null)
                return Json(new { ok = false }, JsonRequestBehavior.AllowGet);

            return Json(new { ok = true, stockQty = data.StockQty, price = data.Price }, JsonRequestBehavior.AllowGet);
        }

        //CustomerModal:
        [HttpGet]
        public ActionResult CustomerCreateModal()
        {
            return PartialView("_CustomerCreateModal", new CustomerCreateVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer(CustomerCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                // رجّع نفس partial مع أخطاء validation
                return PartialView("_CustomerCreateModal", vm);
            }

            var customer = new Customer
            {
                CustomerName = vm.CustomerName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                City = vm.City
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                id = customer.CustomerId,
                name = customer.CustomerName
            });
        }


        #region Dispose

        /// Ensures DbContext is disposed properly.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }

        #endregion
    }
}
