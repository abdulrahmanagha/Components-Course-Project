================================================================================
CAMPUS CAFE ORDERING SYSTEM - CHECKPOINT 1 (DESIGN & RUNNING PROTOTYPE)
================================================================================

1. GROUP & MEMBER DETAILS
-------------------------
Group ID:          G##
Representative ID: 32301002541 (Abdulrahman Najm Gudron-Agha)

Team Members:
- Abdulrahman Najm Gudron-Agha (32301002541) - Representative
- Ehab Ayman Alzaatreh         (32301002083)
- Omar Ahmad Alahmer           (32501002007)
- Saud Gheith Awamleh          (32401002006)

2. ENVIRONMENT & COMPILER SETTINGS
----------------------------------
Platform:        https://dotnetfiddle.net/
Language:        C#
Project Type:    Console
Compiler Option: .NET 8 (Update this line if a different .NET version is selected)
External NuGet Packages: None (Standard .NET libraries only)

3. HOW TO RUN
-------------
1. Open https://dotnetfiddle.net/ in a web browser.
2. Ensure Language is set to "C#", Project Type to "Console", and Compiler to ".NET 8".
3. Replace the entire default editor code with the contents of Program.cs.
4. Click the "Run" button at the top of the page.
5. Observe the non-interactive console output:
   - First, the worked acceptance example receipt (2x P02 Sandwich, 1x P01 Coffee, No Discount) is printed.
   - Second, the automated verification suite executes T01, T04, and T06 and prints PASS/FAIL results with the final summary count.

4. COMPLETED REQUIREMENTS (CHECKPOINT 1)
----------------------------------------
- Logical Component Separation & Dependency Injection:
  * Defined behavioural contracts via interfaces: IProductCatalogue, IPricingPolicy, and IReceiptFormatter.
  * Assembled concrete components in Program.Main (Composition Root) and injected dependencies into OrderService via Constructor Injection.
- Fixed In-Memory Product Catalogue & Immutable Data Models:
  * Implemented InMemoryCatalogue with case-sensitive lookup for P01 (Coffee, 2.50), P02 (Sandwich, 5.00), P03 (Juice, 3.00), and P04 (Cookie, 1.25).
  * Implemented immutable domain/data classes: Product, OrderLine, CalculatedLine, and OrderSummary.
- Order Calculation & No-Discount Policy:
  * Implemented NoDiscount returning the subtotal unchanged and rejecting negative subtotals.
  * Implemented OrderService using decimal arithmetic, policy post-condition guards, and single-step MidpointRounding.AwayFromZero rounding.
- Receipt Formatting & Worked Acceptance Example:
  * Implemented DetailedReceiptFormatter using CultureInfo.InvariantCulture (2 decimal places) without mutating or recalculating OrderSummary values.
  * Demonstrated the worked acceptance example (Subtotal: 12.50, Discount: 0.00, Final Total: 12.50).
- Automated Verification Tests:
  * Implemented headless automated checks in TestRunner for T01 (valid order with NoDiscount), T04 (empty order rejection), and T06 (zero-quantity rejection) with summary counts.
- Architecture & Design Report (Design.pdf):
  * Documented system dependency diagram, Main object assembly, all three interface contracts, four-attribute planned quality matrix, and peer-review contribution table.

5. KNOWN OMISSIONS & LIMITATIONS (DEFERRED TO CHECKPOINTS 2 & 3)
----------------------------------------------------------------
- Pricing Policies: StudentDiscount (10% off) is deferred to Checkpoint 2, and EmployeeDiscount (20% off) is deferred to Checkpoint 3.
- Receipt Formatters: CompactReceiptFormatter is deferred to Checkpoint 3.
- Automated Test Coverage: Only T01, T04, and T06 are executed in Checkpoint 1. Remaining validation, rounding, receipt, and fault-stub tests (T02, T03, T05, T07-T20, plus custom tests) are planned for Checkpoints 2 and 3.
- Persistence & UI: In-memory data and deterministic console scenarios are used by specification; no interactive keyboard input, file storage, or database persistence is implemented.
================================================================================