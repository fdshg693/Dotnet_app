using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAdminTwoFactorAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // 1. DIからUserManagerを取得
        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();
        
        // 2. 現在のユーザーを取得
        var user = await userManager.GetUserAsync(context.HttpContext.User);
        
        // 3. ユーザーがnullまたは認証されていない場合はスキップ（認証ミドルウェアが処理）
        if (user == null)
        {
            await next();
            return;
        }
        
        // 4. 管理者ロールか確認
        var isAdmin = await userManager.IsInRoleAsync(user, "Administrator");
        if (!isAdmin)
        {
            await next();
            return;
        }
        
        // 5. 管理者で2FAが有効になっていない場合、設定ページへリダイレクト
        if (!user.TwoFactorEnabled)
        {
            context.Result = new RedirectToActionResult(
                "Setup", 
                "TwoFactor", 
                new { area = "", returnUrl = context.HttpContext.Request.Path });
            return;
        }
        
        await next();
    }
}
