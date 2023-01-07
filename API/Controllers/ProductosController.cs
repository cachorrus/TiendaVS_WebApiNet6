using API.Dtos;
using API.Helpers;
using API.Helpers.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")] //Por default la version sera 1.0 que se especifico en ApplicationServiceExtension.ConfigureApiVersioning()
[ApiVersion("1.1")]
[ApiVersion("1.2")]
[Authorize(Roles = "Administrador")]
public class ProductosController : BaseAPIController
{
    //Inyeccion de dbContext
    //private readonly TiendaContext _context;

    //public ProductosController(TiendaContext context)
    //{
    //    _context = context;
    //}

    //Usando el patron Repositorio
    //private readonly IProductoRepository _productoRepository;

    //public ProductosController(IProductoRepository productoRepository)
    //{
    //    _productoRepository = productoRepository;
    //}

    //Usando el patron unidad de trabajo
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    //[ApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult<IEnumerable<Producto>>> Get() sin automapper
    public async Task<ActionResult<IEnumerable<ProductoListDto>>> Get()
    {        
        //Usando el patron repositorio
        //var productos = await _productoRepository.GetAllAsync();

        //Usando el patron unidad de trabajo
        var productos = await _unitOfWork.Productos.GetAllAsync();

        //return Ok(productos);
        return _mapper.Map<List<ProductoListDto>>(productos);
    }

    //GET: api/productos?ver=1.1
    [HttpGet]
    //[ApiVersion("1.1")]
    //Al cambiar las versiones soportadas a nivel de controlador la version del endpoint se especifica asi
    [MapToApiVersion("1.1")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult<IEnumerable<Producto>>> Get() sin automapper
    public async Task<ActionResult<IEnumerable<ProductoDto>>> Get11()
    {
        //Usando el patron repositorio
        //var productos = await _productoRepository.GetAllAsync();

        //Usando el patron unidad de trabajo
        var productos = await _unitOfWork.Productos.GetAllAsync();

        //return Ok(productos);
        return _mapper.Map<List<ProductoDto>>(productos);
    }

    //GET: api/productos?ver=1.2&pageIndex=4&pageSize=3&search=Yamaha
    [HttpGet]
    [MapToApiVersion("1.2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult<IEnumerable<Producto>>> Get() sin automapper
    public async Task<ActionResult<Pager<ProductoListDto>>> Get12([FromQuery] Params productoParams)
    {
        //Usando el patron unidad de trabajo
        var resultado = await _unitOfWork.Productos.GetAllAsync(productoParams.PageIndex,productoParams.PageSize, productoParams.Search);

        var listadoProductosDto = _mapper.Map<List<ProductoListDto>>(resultado.registros);

        //Si quisieramos retornar informacion de la paginacion en los Headers
        Response.Headers.Add("X-InLineCount", resultado.totalRegistros.ToString());

        return new Pager<ProductoListDto>(listadoProductosDto, resultado.totalRegistros, productoParams.PageIndex, productoParams.PageSize, productoParams.Search);
    }
    
    //GET: api/productos/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> Get(int id) sin automapper
    public async Task<ActionResult<ProductoDto>> Get(int id)
    {
        //Usando directamente dbContext
        //var producto = await _context.Productos.FindAsync(id);
        
        //Usando el patron repositorio
        //var producto = await _productoRepository.GetByIdAsync(id);

        //Usando la unidad de trabajo
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);

        if (producto == null)
        {
            return NotFound(new ApiResponse(404, "El producto solicitado no existe."));
        }

        //return Ok(producto);
        return _mapper.Map<ProductoDto>(producto);
    }

    //POST: api/productos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoAddUpdateDto>> Post(ProductoAddUpdateDto productoAddUpdateDto)
    {
        var producto = _mapper.Map<Producto>(productoAddUpdateDto);

        _unitOfWork.Productos.Add(producto);
        await _unitOfWork.SaveAsync();

        if (producto == null)
        {
            return BadRequest(new ApiResponse(400));
        }
        productoAddUpdateDto.Id = producto.Id;
        return CreatedAtAction(nameof(Post), new { id = productoAddUpdateDto.Id }, productoAddUpdateDto);
    }
    
    //PUT: api/productos/28
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoAddUpdateDto>> Put(int id,[FromBody]ProductoAddUpdateDto productoAddUpdateDto)
    {
        if (productoAddUpdateDto == null)
        {
            return NotFound(new ApiResponse(404, "El producto solicitado no existe."));
        }

        var productoBD = await _unitOfWork.Productos.GetByIdAsync(id);

        if (productoBD == null)
        {
            return BadRequest(new ApiResponse(400, "El producto solicitado no existe."));
        }

        var producto = _mapper.Map<Producto>(productoAddUpdateDto);

        _unitOfWork.Productos.Update(producto);
        await _unitOfWork.SaveAsync();

        return productoAddUpdateDto;
    }
    
    //DELETE: api/productos/28
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id);

        if (producto == null)
        {
            return NotFound(new ApiResponse(404, "El producto solicitado no existe."));
        }

        _unitOfWork.Productos.Remove(producto);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }
    
}
