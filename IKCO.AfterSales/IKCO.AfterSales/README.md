# سامانه مدیریت خدمات پس از فروش (IKCO After-Sales)

پروژه نمونه ویندوز فرم به زبان C# که به‌عنوان **مبدأ مهاجرت** برای تبدیل به وب
(بک‌اند ASP.NET Core / ABP و فرانت React) ساخته شده است.

پروژه عمداً در اندازه‌ای طراحی شده که هم الگوهای واقعی یک سیستم سازمانی را داشته باشد
و هم آن‌قدر بزرگ نباشد که مهاجرت آزمایشی طول بکشد.

---

## ۱. پیش‌نیازها

| مورد | نسخه |
|---|---|
| SQL Server | 2016 یا بالاتر (Express هم کافی است) |
| .NET Framework Developer Pack | 4.8 |
| Visual Studio 2022 یا VS Code + `dotnet` CLI | — |
| سیستم‌عامل | ویندوز (به دلیل Windows Forms) |

---

## ۲. راه‌اندازی پایگاه داده

اسکریپت‌ها را به همین ترتیب اجرا کنید:

```
Database/01_Schema.sql            -- ساخت دیتابیس و جداول
Database/02_StoredProcedures.sql  -- تمام رویه‌های ذخیره‌شده
Database/03_SeedData.sql          -- داده نمونه
```

با `sqlcmd`:

```bash
sqlcmd -S . -E -i Database/01_Schema.sql
sqlcmd -S . -E -i Database/02_StoredProcedures.sql
sqlcmd -S . -E -i Database/03_SeedData.sql
```

سپس رشته اتصال را در `src/IKCO.AfterSales.WinForms/App.config` تنظیم کنید:

```xml
<add name="AfterSalesDb"
     connectionString="Data Source=.;Initial Catalog=IKCO_AfterSales;Integrated Security=True" />
```

---

## ۳. اجرا

```bash
dotnet build IKCO.AfterSales.sln
dotnet run --project src/IKCO.AfterSales.WinForms
```

یا فایل `IKCO.AfterSales.sln` را در Visual Studio باز کرده و F5 بزنید.

---

## ۴. ساختار پروژه

```
IKCO.AfterSales/
├── Database/                     اسکریپت‌های SQL Server
├── src/IKCO.AfterSales.WinForms/
│   ├── Models/                   کلاس‌های POCO (بدون منطق)
│   ├── Data/                     لایه دسترسی به داده
│   │   ├── SqlHelper.cs          پوشش نازک روی ADO.NET
│   │   └── *Repository.cs        یک ریپازیتوری برای هر موجودیت
│   ├── Common/                   تنظیمات، پیام‌ها، اعتبارسنجی، تاریخ شمسی
│   ├── Forms/                    فرم‌های ویندوزی
│   ├── Program.cs
│   └── App.config
└── IKCO.AfterSales.sln
```

### موجودیت‌ها

| جدول | توضیح |
|---|---|
| `Customers` | مشتریان |
| `Vehicles` | خودروها (متعلق به مشتری) |
| `Technicians` | تعمیرکاران با نرخ ساعتی |
| `Parts` | قطعات با موجودی انبار |
| `ServiceRequests` | درخواست تعمیر (سند اصلی) |
| `ServiceRequestParts` | قطعات مصرفی هر درخواست (ردیف‌های سند) |
| `ServiceStatuses` | وضعیت‌های چرخه کار |

### فرم‌ها

| فرم | نوع |
|---|---|
| `MainForm` | پوسته MDI و منوی اصلی |
| `CustomerListForm` / `CustomerEditForm` | CRUD ساده + اعتبارسنجی کد ملی |
| `VehicleListForm` / `VehicleEditForm` | CRUD با کلید خارجی |
| `TechnicianListForm` / `TechnicianEditForm` | CRUD ساده |
| `PartListForm` / `PartEditForm` | CRUD + هایلایت کسری موجودی |
| `ServiceRequestListForm` | لیست با فیلتر چندگانه و صفحه‌بندی |
| `ServiceRequestEditForm` | **سند اصلی: master-detail** |
| `ReportsForm` | سه گزارش تجمیعی روی TabControl |

---

## ۵. قواعد کسب‌وکار (مهم برای مهاجرت)

منطق در دو جا پخش شده است — دقیقاً مثل سیستم‌های واقعی. هنگام مهاجرت باید مشخص شود
هرکدام در معماری جدید کجا می‌نشیند.

**داخل رویه‌های ذخیره‌شده:**

1. تولید خودکار شماره درخواست به فرمت `SR-{year}-{seq}` (`usp_ServiceRequest_Insert`)
2. کسر موجودی انبار هنگام افزودن قطعه، همراه با کنترل کفایت موجودی (`usp_ServiceRequestPart_Add`)
3. بازگشت موجودی هنگام حذف ردیف یا حذف کل درخواست
4. محاسبه مجدد هزینه‌ها: `TotalCost = LaborHours × HourlyRate + PartsCost − Discount` (`usp_ServiceRequest_Recalculate`)
5. ممانعت از حذف مشتری دارای خودرو، خودرو دارای درخواست، و قطعه استفاده‌شده
6. ممانعت از ویرایش یا حذف درخواست در وضعیت نهایی
7. ثبت خودکار `CompletedDate` هنگام تغییر وضعیت به «تکمیل شده»

**داخل کد فرم (سخت‌ترین بخش مهاجرت):**

8. **تخفیف وفاداری** — اگر مشتری بیش از `LoyaltyThreshold` درخواست تکمیل‌شده داشته باشد،
   `LoyaltyDiscountPercent` درصد از دستمزد پیشنهاد و از کاربر تأیید گرفته می‌شود.
   این قاعده در `ServiceRequestEditForm.CalculateSuggestedDiscount` است، نه در دیتابیس.
9. **قفل شدن فرم در وضعیت نهایی** — `ApplyFinalStateLock` کنترل‌ها را غیرفعال می‌کند.
   یعنی همان قاعده هم در UI و هم در SP تکرار شده است.
10. اعتبارسنجی ورودی‌ها (کد ملی، موبایل، سال تولید) در `ValidationHelper`.

> این تکرار و پراکندگی عمدی است. هدف این است که مهاجرت آزمایشی نشان دهد
> ابزار با منطقی که در کد UI دفن شده چه می‌کند.

---

## ۶. نکات فنی مرتبط با مهاجرت

- تمام دسترسی به داده از طریق **Stored Procedure** است؛ هیچ SQL درون‌خطی وجود ندارد.
- خطاهای کسب‌وکار با `THROW` و کدهای ۵۰۰۰۱ به بالا برگردانده می‌شوند و در
  `UiHelper.ShowError` به پیام کاربر تبدیل می‌شوند.
- تاریخ‌ها در دیتابیس **میلادی** ذخیره و در UI **شمسی** نمایش داده می‌شوند
  (`PersianDateHelper`). این تبدیل باید در نسخه وب هم حفظ شود.
- صفحه‌بندی سمت سرور با `OFFSET/FETCH` و پارامتر خروجی `@TotalCount` انجام می‌شود.
- رابط کاربری **راست‌به‌چپ** است.
- هیچ وابستگی NuGet خارجی وجود ندارد.

---

## ۷. آنچه پروژه عمداً ندارد

اینها در نسخه وب باید اضافه شوند و بخشی از تصمیم‌های معماری مهاجرت هستند:

- احراز هویت و سطح دسترسی
- لاگ عملیات و ردگیری تغییرات
- تست خودکار
- چندکاربره بودن همزمان
