using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models.Common;
using Shikhsa.Models.Notification;

public class NotificationTemplateRepository 
{
    private readonly ApplicationDbContext _context;

    public NotificationTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }
#region Template
    public async Task<List<NotificationTemplate>> GetAllAsync()
    {
        return await _context.NotificationTemplates
            .Include(x => x.NotificationTemplateCategories)
            .OrderBy(x => x.TemplateName)
            .ToListAsync();
    }

    public async Task<NotificationTemplate?> GetByIdAsync(long id)
    {
        return await _context.NotificationTemplates
            .Include(x => x.NotificationTemplateCategories)
            .FirstOrDefaultAsync(x => x.NotificationTemplateId == id);
    }

    public async Task<bool> IsCodeExistsAsync(string templateCode, long id)
    {
        return await _context.NotificationTemplates
            .AnyAsync(x => x.TemplateCode == templateCode &&
                           x.NotificationTemplateId != id);
    }

    public async Task<ResponseModel> SaveAsync(NotificationTemplate model)
    {
        ResponseModel response = new();

        try
        {
            if (await IsCodeExistsAsync(model.TemplateCode, model.NotificationTemplateId))
            {
                response.Status = 0;
                response.Message = "Template code already exists.";
                return response;
            }

            if (model.NotificationTemplateId == 0)
            {
                _context.NotificationTemplates.Add(model);
                response.Message = "Template created successfully.";
            }
            else
            {
                _context.NotificationTemplates.Update(model);
                response.Message = "Template updated successfully.";
            }

            await _context.SaveChangesAsync();

            response.Status = 1;
        }
        catch (Exception ex)
        {
            response.Status = 0;
            response.Message = ex.Message;
        }
        response.Id = model.NotificationTemplateId;

        return response;
    }

    public async Task<ResponseModel> DeleteAsync(long id)
    {
        ResponseModel response = new();

        var entity = await _context.NotificationTemplates.FindAsync(id);

        if (entity == null)
        {
            response.Status = 0;
            response.Message = "Record not found.";
            return response;
        }

        _context.NotificationTemplates.Remove(entity);

        await _context.SaveChangesAsync();

        response.Status = 1;
        response.Message = "Deleted Successfully.";

        return response;
    }
    #endregion
    #region Category
    public async Task<List<NotificationCategory>> GetAllCategoryAsync()
    {
        return await _context.NotificationCategories
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .ToListAsync();
    }

    public async Task<NotificationCategory?> GetCategoryByIdAsync(long id)
    {
        return await _context.NotificationCategories
            .FirstOrDefaultAsync(x => x.NotificationCategoryId == id);
    }

    public async Task<bool> IsCategoryCodeExistsAsync(string code, long id)
    {
        return await _context.NotificationCategories
            .AnyAsync(x =>
                x.CategoryCode == code &&
                x.NotificationCategoryId != id);
    }

    public async Task<bool> IsNameExistsAsync(string name, long id)
    {
        return await _context.NotificationCategories
            .AnyAsync(x =>
                x.CategoryName == name &&
                x.NotificationCategoryId != id);
    }

    public async Task<ResponseModel> SaveCategoryAsync(NotificationCategory model)
    {
        ResponseModel response = new();

        if (await IsCodeExistsAsync(model.CategoryCode, model.NotificationCategoryId))
        {
            response.Status = 0;
            response.Message = "Category code already exists.";
            return response;
        }

        if (await IsNameExistsAsync(model.CategoryName, model.NotificationCategoryId))
        {
            response.Status = 0;
            response.Message = "Category name already exists.";
            return response;
        }

        try
        {
            if (model.NotificationCategoryId == 0)
            {
                _context.NotificationCategories.Add(model);

                response.Message = "Category created successfully.";
            }
            else
            {
                var entity = await _context.NotificationCategories
                    .FirstOrDefaultAsync(x =>
                        x.NotificationCategoryId == model.NotificationCategoryId);

                if (entity == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                entity.CategoryCode = model.CategoryCode;
                entity.CategoryName = model.CategoryName;
                entity.Icon = model.Icon;
                entity.Color = model.Color;
                entity.DisplayOrder = model.DisplayOrder;
                entity.IsActive = model.IsActive;

                response.Message = "Category updated successfully.";
            }

            await _context.SaveChangesAsync();

            response.Status = 1;
        }
        catch (Exception ex)
        {
            response.Status = 0;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseModel> DeleteCategoryAsync(long id)
    {
        ResponseModel response = new();

        var entity = await _context.NotificationCategories
            .FirstOrDefaultAsync(x =>
                x.NotificationCategoryId == id);

        if (entity == null)
        {
            response.Status = 0;
            response.Message = "Record not found.";
            return response;
        }

        _context.NotificationCategories.Remove(entity);

        await _context.SaveChangesAsync();

        response.Status = 1;
        response.Message = "Deleted successfully.";

        return response;
    }
    #endregion
    #region Placeholder

    public async Task<List<NotificationPlaceholder>> GetPaceHolderAllAsync()
    {
        return await _context.NotificationPlaceholders
            .Include(x => x.NotificationCategory)
            .OrderBy(x => x.NotificationCategory.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();
    }
    public async Task<NotificationPlaceholder?> GetPlaceHolderByIdAsync(long id)
    {
        return await _context.NotificationPlaceholders
            .FirstOrDefaultAsync(x =>
                x.NotificationPlaceholderId == id);
    }
    public async Task<bool> IsPlaceHolderCodeExistsAsync(string code, long id)
    {
        return await _context.NotificationPlaceholders
            .AnyAsync(x =>
                x.PlaceholderCode == code &&
                x.NotificationPlaceholderId != id);
    }
    public async Task<List<NotificationCategory>> GetCategoriesAsync()
    {
        return await _context.NotificationCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .ToListAsync();
    }
    public async Task<ResponseModel> SaveAsync(NotificationPlaceholder model)
    {
        ResponseModel response = new();

        if (await IsCodeExistsAsync(model.PlaceholderCode,
                                    model.NotificationPlaceholderId))
        {
            response.Status = 0;
            response.Message = "Placeholder already exists.";
            return response;
        }

        try
        {
            if (model.NotificationPlaceholderId == 0)
            {
                _context.NotificationPlaceholders.Add(model);

                response.Message = "Placeholder created successfully.";
            }
            else
            {
                var entity = await _context.NotificationPlaceholders
                    .FirstOrDefaultAsync(x =>
                        x.NotificationPlaceholderId ==
                        model.NotificationPlaceholderId);

                if (entity == null)
                {
                    response.Status = 0;
                    response.Message = "Record not found.";
                    return response;
                }

                entity.NotificationCategoryId = model.NotificationCategoryId;
                entity.PlaceholderCode = model.PlaceholderCode;
                entity.DisplayName = model.DisplayName;
                entity.Description = model.Description;
                entity.SampleValue = model.SampleValue;
                entity.DisplayOrder = model.DisplayOrder;
                entity.IsActive = model.IsActive;

                response.Message = "Placeholder updated successfully.";
            }

            await _context.SaveChangesAsync();

            response.Status = 1;
        }
        catch (Exception ex)
        {
            response.Status = 0;
            response.Message = ex.Message;
        }

        return response;
    }
    public async Task<ResponseModel> DeletePlaceHolderAsync(long id)
    {
        ResponseModel response = new();

        var entity = await _context.NotificationPlaceholders
            .FirstOrDefaultAsync(x =>
                x.NotificationPlaceholderId == id);

        if (entity == null)
        {
            response.Status = 0;
            response.Message = "Record not found.";
            return response;
        }

        _context.NotificationPlaceholders.Remove(entity);

        await _context.SaveChangesAsync();

        response.Status = 1;
        response.Message = "Deleted successfully.";

        return response;
    }
    #endregion
}

