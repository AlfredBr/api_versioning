# API Versioning Demo

An ASP.NET Core minimal API demonstration showcasing **API versioning**, and **Swagger integration**.

![API Versioning Demo](screenshot.png)

## 📁 Project Structure

```
api_versioning/
├── Program.cs                # Main application and API endpoints
├── api_versioning.http       # Comprehensive test suite
├── api_versioning.csproj     # Project dependencies
├── README.md                 # This file
```

### 💡 Works with this configuration ('cause the dropdown did not work in later versions of Swashbuckle)
```xml
<ItemGroup>
    <PackageReference Include="Asp.Versioning.Http" Version="8.1.0" />
    <PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.5" />
    <PackageReference Include="Swashbuckle.AspNetCore.Swagger" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="6.5.0" />
</ItemGroup>
```


## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Built with ❤️ using ASP.NET Core Minimal APIs