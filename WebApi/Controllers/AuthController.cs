﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    public AuthController(
        ILogger<AuthController> logger,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }
    
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel payload)
    {
        if (!ModelState.IsValid) return BadRequest();
        var user = await _userManager.FindByNameAsync(payload.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, payload.Password)) 
            return Unauthorized();
            
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
            
        authClaims
            .AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

        var token = GetToken(authClaims);
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse()
        {
            Token = jwtToken,
            BearerToken = $"Bearer {jwtToken}",
            Expiration = token.ValidTo,
            Success = true
        });

    }
    
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel payload)
    {
        var userExists = await _userManager.FindByNameAsync(payload.Username);
        if (userExists is not null) return BadRequest("User already exists!");
        
        var user = new User()
        {
            Email = payload.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = payload.Username
        };
        
        var result = await _userManager.CreateAsync(user, payload.Password);
        return result.Succeeded
            ? Created()
            : StatusCode(StatusCodes.Status500InternalServerError,
                new Response() { Success = false, Message = $"Failed to create user: {string.Join(',', result.Errors.Select(e => e.Code))}" });
    }
    
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}