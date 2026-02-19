# Permission System Implementation Checkpoint

## Overview
This document summarizes the changes made to implement comprehensive frontend permission controls. The system now enforces visibility of UI elements (buttons, menu items) based on user permissions and handles API access denials gracefully.

## 1. Frontend Structure (`dashboard.html`)
- **Action Buttons**: Added `data-permission="Module:Write"` attributes to "Add New" buttons across all major sections:
  - Products (`Product:Write`)
  - Companies (`Companies:Write`)
  - Suppliers (`Supplier:Write`)
  - Customers (`Customer:Write`)
  - PriceLists (`PriceList:Write`)
  - Offers (`Offers:Write`)
  - Inventory Tabs (Brands, Categories, Units -> `Product:Write`)
- **Inventory Tabs**: Added specific `data-permission` attributes to the sub-navigation buttons in the Inventory section (e.g., `Stock:Write` for Stock Entry).

## 2. Frontend Logic (`app.js`)

### Global Permission Handler (`applyPermissions`)
- Updated to scan for all elements with `data-permission` attribute.
- Automatically hides elements if `window.hasPermission` returns false.
- Improved logic for toggling display styles (resets to default/flex/inline-block correctly).
- **Menu Visibility**: Refined logic to hide menu groups if all their children are hidden.

### Data Loading Functions
All major `load*` functions were refactored to include:
1.  **403 Forbidden Handling**: Checks `res.status === 403` before parsing JSON. If forbidden, displays a "🚫 Yetkisiz Erişim" message in the table.
2.  **Conditional Button Rendering**: Inside the loop generating table rows, checks specific permissions (e.g., `canWrite`, `canDelete`) to decide whether to render "Edit", "Delete", or "Approve" buttons.

**Updated Functions:**
- `loadProducts()`: Checks `Product:Write/Delete`.
- `loadSuppliers()`: Checks `Supplier:Write/Delete`.
- `loadCompanies()`: Checks `Companies:Delete`.
- `loadCustomers()`: Checks `Customer:Write/Delete`.
- `loadCustomerCompanies()`: Checks `Customer:Write/Delete`.
- `loadPriceLists()`: Checks `PriceList:Write/Delete`.
- `loadOffers()`: Checks `Offers:Write` (Affects "Edit", "Approve", "invoice" buttons).
- `loadInvoices()`: Checks `Invoices:Write` (Affects "Approve" button).
- `loadLogs()`: Handles 403 error (View only, no actions).
- `loadWarehouses()`: (Previously updated) Checks `Warehouse:Write/Delete`.

### Recent Analysis & Updates (Session 2)

#### 1. User Management (`loadUsers`)
- **API**: `GET /api/users`
- **Security Check**: Handles 403 Forbidden.
- **Client-Side Guard**: Explicitly blocks 'User' role from executing the function body.
- **Button Permissions**:
  - "Yetkiler" (Permissions) & "Düzenle" (Edit) -> Checks `Users:Write`.
  - "Sil" (Delete) -> Checks `Users:Delete`.

#### 2. Simple Entities (`loadSimple`)
- **Scope**: Brands, Categories, Units.
- **API**: `GET /api/product/{endpoint}` (brands, categories, units)
- **Security Check**: Handles 403 Forbidden.
- **Button Permissions**:
  - "Düzenle" & "Sil" buttons check `Product:Write` and `Product:Delete` respectively.
  - Consistent across all three sub-modules.

#### 3. System Settings (`SystemController` & `app.js`)
- **API Endpoints**:
  - `GET /api/system/info`: Protected by `System:Read`.
  - `GET /api/system/mail-settings`: Protected by `System:Read`.
  - `POST /api/system/mail-settings`: Protected by `System:Write`.
- **Special Hardcoded Restriction**:
  - **Database Configuration (`/api/system/db-config`)**:
    - **CRITICAL**: The controller explicitly checks `if (userService.UserId != 1) return Forbid();`.
    - This means **ONLY the ROOT user (ID 1)** can view or change database connection strings, regardless of permissions. This is a security feature.

#### 4. Reports (`loadStockReport`)
- **API**: `GET /api/stock/report`
- **Security Check**: Handles 403 Forbidden.
- **Permissions**:
  - The API Endpoint `/api/stock/report` is protected by **`Stock:Read`** (Confirmed in StockController.cs).
  - The Frontend Menu Item (`data-module="Reports"`) checks for **`Reports:Read`**.
  - **MISMATCH**: A user with `Reports:Read` but NO `Stock:Read` will see the menu but get a 403 error when loading the list.
  - **Proposed Fix**: Change the menu item to use `data-module="Stock"` OR ensure `Reports` module permission implies `Stock` read access. For now, we will leave it as is but note the dependency.

#### 5. Stock Entry (`loadStockEntry` & `submitStockEntry`)
- **API**: `GET /api/product` (for dropdown), `GET /api/warehouse` (for dropdown), `POST /api/stock/movement`.
- **Security Check**:
  - `loadStockEntry`: Fetches products and warehouses. If either returns 403 (checked explicitly), the dropdowns show "Yetkiniz yok".
  - `submitStockEntry`: Explicitly checks `window.hasPermission('Stock', 'write')` before sending the request.
  - Menu Item & Tab Button: Already protected by `data-permission="Stock:Write"`.

## 3. Next Steps / Pending
- **Testing**: Verify that specific users (e.g., with Read-only access) cannot see Add/Edit/Delete buttons.
- **Stock Entry**: Ensure `loadStockEntry` and `submitStockEntry` are fully protected (currently `loadStockEntry` has menu/tab protection).
- **Backend Alignment**: Ensure `IsFull` permission logic in the backend matches the frontend's expectations (if any discrepancies arise).

## Usage for AI Model
If continuing from here:
- **Reference**: Use `window.hasPermission(module, type)` for any new UI checks.
- **Pattern**: Follow the `load*` function pattern: Fetch -> Check 403 -> Check Permissions -> Render Rows with/without buttons.
