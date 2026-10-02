-- Promociones: descuento por porcentaje configurable por el admin, aplicado a un módulo
-- completo (todas sus categorías con precio) o a una sola categoría — nunca a ambos.
--
-- module_id is NO ACTION on purpose: SQL Server rejects two cascade paths into one table
-- (modules -> promotions directly AND modules -> categories -> promotions), error 1785.
-- ModuleRepository.DeleteAsync deletes a module's promotions itself, in the same transaction.

CREATE TABLE promotions (
    id                    UNIQUEIDENTIFIER PRIMARY KEY,
    module_id             UNIQUEIDENTIFIER NULL REFERENCES modules (id) ON DELETE NO ACTION,
    category_id           UNIQUEIDENTIFIER NULL REFERENCES categories (id) ON DELETE CASCADE,
    label                 NVARCHAR(200) NOT NULL,
    discount_percentage   NUMERIC(5, 2) NOT NULL,
    is_active             BIT NOT NULL DEFAULT 1,
    created_at_utc        DATETIMEOFFSET NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATETIMEOFFSET),
    CONSTRAINT ck_promotions_exactly_one_target CHECK (
        (module_id IS NOT NULL AND category_id IS NULL) OR (module_id IS NULL AND category_id IS NOT NULL)
    ),
    CONSTRAINT ck_promotions_discount_range CHECK (discount_percentage > 0 AND discount_percentage <= 100)
);

CREATE INDEX ix_promotions_module_active ON promotions (module_id, is_active);
CREATE INDEX ix_promotions_category_active ON promotions (category_id, is_active);
