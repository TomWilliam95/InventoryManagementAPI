using InventoryManagementAPI.Models.CoreModels;
using InventoryManagementAPI.Models.DTO_s.SupplierDTO_s;
using InventoryManagementAPI.Models.DTO_s.SupplierContactDTO_s;
using InventoryManagementAPI.Models.DTO_s.SupplierAddressDTO_s;
using InventoryManagementAPI.Models.DTO_s.SupplierProductDTO_s;
using InventoryManagementAPI.Models.Enums;
using InventoryManagementAPI.Repositories.SupplierRepositories;
using InventoryManagementAPI.Repositories.SupplierContactRepositories;
using InventoryManagementAPI.Repositories.SupplierAddressRepositories;
using InventoryManagementAPI.Repositories.SupplierProductRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementAPI.Controllers;

[Route("api/suppliers")]
[ApiController]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _suppliers; private readonly ISupplierContactService _contacts; private readonly ISupplierAddressService _addresses; private readonly ISupplierProductService _products;
    public SupplierController(ISupplierService suppliers, ISupplierContactService contacts, ISupplierAddressService addresses, ISupplierProductService products) { _suppliers = suppliers; _contacts = contacts; _addresses = addresses; _products = products; }

    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) { var r=await _suppliers.GetAllSuppliersAsync(ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("{supplierId:int}")] public async Task<IActionResult> Get(int supplierId,CancellationToken ct) { var r=await _suppliers.GetSupplierByIdAsync(supplierId,ct); return StatusCode(r.StatusCode,r); }
    [HttpPost,Authorize(Policy="AdminOrManager")] public async Task<IActionResult> Create(CreateSupplierRequestDTO dto,CancellationToken ct) { var r=await _suppliers.CreateSupplierAsync(dto,ct); return r.StatusCode==201&&r.Data!=null?CreatedAtAction(nameof(Get),new{supplierId=r.Data.ID},r):StatusCode(r.StatusCode,r); }
    [HttpPut("{supplierId:int}"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> Update(int supplierId,UpdateSupplierRequestDTO dto,CancellationToken ct) { var r=await _suppliers.UpdateSupplierAsync(supplierId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/activate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> Activate(int supplierId,UpdateSupplierStatusRequestDTO dto,CancellationToken ct) { var r=await _suppliers.ActivateSupplierAsync(supplierId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/deactivate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> Deactivate(int supplierId,UpdateSupplierStatusRequestDTO dto,CancellationToken ct) { var r=await _suppliers.DeactivateSupplierAsync(supplierId,dto,ct); return StatusCode(r.StatusCode,r); }

    [HttpGet("{supplierId:int}/contacts")] public async Task<IActionResult> GetContacts(int supplierId,CancellationToken ct) { var r=await _contacts.GetAllAsync(supplierId,ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("{supplierId:int}/contacts/primary")] public async Task<IActionResult> GetPrimaryContact(int supplierId,CancellationToken ct) { var r=await _contacts.GetPrimaryAsync(supplierId,ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("{supplierId:int}/contacts/{contactId:int}")] public async Task<IActionResult> GetContact(int supplierId,int contactId,CancellationToken ct) { var r=await _contacts.GetByIdAsync(supplierId,contactId,ct); return StatusCode(r.StatusCode,r); }
    [HttpPost("{supplierId:int}/contacts"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> CreateContact(int supplierId,CreateSupplierContactRequestDTO dto,CancellationToken ct) { var r=await _contacts.CreateAsync(supplierId,dto,ct); return r.StatusCode==201&&r.Data!=null?CreatedAtAction(nameof(GetContact),new{supplierId,contactId=r.Data.ID},r):StatusCode(r.StatusCode,r); }
    [HttpPut("{supplierId:int}/contacts/{contactId:int}"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> UpdateContact(int supplierId,int contactId,UpdateSupplierContactRequestDTO dto,CancellationToken ct) { var r=await _contacts.UpdateAsync(supplierId,contactId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/contacts/{contactId:int}/primary"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> SetPrimaryContact(int supplierId,int contactId,UpdateSupplierContactPrimaryRequestDTO dto,CancellationToken ct) { var r=await _contacts.SetPrimaryAsync(supplierId,contactId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/contacts/{contactId:int}/activate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> ActivateContact(int supplierId,int contactId,UpdateSupplierContactStatusRequestDTO dto,CancellationToken ct) { var r=await _contacts.ActivateAsync(supplierId,contactId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/contacts/{contactId:int}/deactivate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> DeactivateContact(int supplierId,int contactId,UpdateSupplierContactStatusRequestDTO dto,CancellationToken ct) { var r=await _contacts.DeactivateAsync(supplierId,contactId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpDelete("{supplierId:int}/contacts/{contactId:int}"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> DeleteContact(int supplierId,int contactId,DeleteSupplierContactRequestDTO dto,CancellationToken ct) { var r=await _contacts.DeleteAsync(supplierId,contactId,dto,ct); return StatusCode(r.StatusCode,r); }

    [HttpGet("{supplierId:int}/addresses")] public async Task<IActionResult> GetAddresses(int supplierId,CancellationToken ct) { var r=await _addresses.GetAllAsync(supplierId,ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("{supplierId:int}/addresses/types/{type}")] public async Task<IActionResult> GetAddressesByType(int supplierId,SupplierAddressType type,CancellationToken ct) { var r=await _addresses.GetByTypeAsync(supplierId,type,ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("{supplierId:int}/addresses/{addressId:int}")] public async Task<IActionResult> GetAddress(int supplierId,int addressId,CancellationToken ct) { var r=await _addresses.GetByIdAsync(supplierId,addressId,ct); return StatusCode(r.StatusCode,r); }
    [HttpPost("{supplierId:int}/addresses"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> CreateAddress(int supplierId,CreateSupplierAddressRequestDTO dto,CancellationToken ct) { var r=await _addresses.CreateAsync(supplierId,dto,ct); return r.StatusCode==201&&r.Data!=null?CreatedAtAction(nameof(GetAddress),new{supplierId,addressId=r.Data.ID},r):StatusCode(r.StatusCode,r); }
    [HttpPut("{supplierId:int}/addresses/{addressId:int}"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> UpdateAddress(int supplierId,int addressId,UpdateSupplierAddressRequestDTO dto,CancellationToken ct) { var r=await _addresses.UpdateAsync(supplierId,addressId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/addresses/{addressId:int}/primary"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> SetPrimaryAddress(int supplierId,int addressId,UpdateSupplierAddressPrimaryRequestDTO dto,CancellationToken ct) { var r=await _addresses.SetPrimaryAsync(supplierId,addressId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/addresses/{addressId:int}/activate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> ActivateAddress(int supplierId,int addressId,UpdateSupplierAddressStatusRequestDTO dto,CancellationToken ct) { var r=await _addresses.ActivateAsync(supplierId,addressId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/addresses/{addressId:int}/deactivate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> DeactivateAddress(int supplierId,int addressId,UpdateSupplierAddressStatusRequestDTO dto,CancellationToken ct) { var r=await _addresses.DeactivateAsync(supplierId,addressId,dto,ct); return StatusCode(r.StatusCode,r); }

    [HttpGet("{supplierId:int}/products")] public async Task<IActionResult> GetSupplierProducts(int supplierId,CancellationToken ct) { var r=await _products.GetAllBySupplierAsync(supplierId,ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("~/api/products/{productId:int}/suppliers")] public async Task<IActionResult> GetProductSuppliers(int productId,CancellationToken ct) { var r=await _products.GetAllByProductAsync(productId,ct); return StatusCode(r.StatusCode,r); }
    [HttpGet("{supplierId:int}/products/{productId:int}")] public async Task<IActionResult> GetSupplierProduct(int supplierId,int productId,CancellationToken ct) { var r=await _products.GetAsync(supplierId,productId,ct); return StatusCode(r.StatusCode,r); }
    [HttpPost("{supplierId:int}/products"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> AssignProduct(int supplierId,CreateSupplierProductRequestDTO dto,CancellationToken ct) { var r=await _products.AssignAsync(supplierId,dto,ct); return r.StatusCode==201&&r.Data!=null?CreatedAtAction(nameof(GetSupplierProduct),new{supplierId,productId=r.Data.ProductID},r):StatusCode(r.StatusCode,r); }
    [HttpPut("{supplierId:int}/products/{productId:int}"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> UpdateSupplierProduct(int supplierId,int productId,UpdateSupplierProductRequestDTO dto,CancellationToken ct) { var r=await _products.UpdateAsync(supplierId,productId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/products/{productId:int}/preferred"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> SetPreferred(int supplierId,int productId,UpdateSupplierProductPreferredRequestDTO dto,CancellationToken ct) { var r=await _products.SetPreferredAsync(supplierId,productId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/products/{productId:int}/activate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> ActivateSupplierProduct(int supplierId,int productId,UpdateSupplierProductStatusRequestDTO dto,CancellationToken ct) { var r=await _products.ActivateAsync(supplierId,productId,dto,ct); return StatusCode(r.StatusCode,r); }
    [HttpPatch("{supplierId:int}/products/{productId:int}/deactivate"),Authorize(Policy="AdminOrManager")] public async Task<IActionResult> DeactivateSupplierProduct(int supplierId,int productId,UpdateSupplierProductStatusRequestDTO dto,CancellationToken ct) { var r=await _products.DeactivateAsync(supplierId,productId,dto,ct); return StatusCode(r.StatusCode,r); }
}
