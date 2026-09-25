-- Precios por categoría (usados por el cotizador tipo carrito del frontend) y un nuevo
-- módulo de personajes/shows — botargas, robot y Pajara Peggy son artículos reales que se
-- arrastran al presupuesto igual que una torre de luces.

ALTER TABLE categories ADD COLUMN price NUMERIC(10, 2) NULL;

-- "Desde" precio en MXN. NULL = va incluido al contratar el módulo (p. ej. la música del DJ),
-- no es un artículo independiente que se pueda agregar al carrito.
UPDATE categories SET price = 1800.00 WHERE id = '22222222-0000-0000-0000-000000000001'; -- Torre de luces
UPDATE categories SET price = 900.00  WHERE id = '22222222-0000-0000-0000-000000000002'; -- Reflectores
UPDATE categories SET price = 1500.00 WHERE id = '22222222-0000-0000-0000-000000000003'; -- LED RGB
UPDATE categories SET price = 1200.00 WHERE id = '22222222-0000-0000-0000-000000000004'; -- Láser
UPDATE categories SET price = NULL    WHERE id = '22222222-0000-0000-0000-000000000005'; -- Géneros (incluido)
UPDATE categories SET price = NULL    WHERE id = '22222222-0000-0000-0000-000000000006'; -- Mezclas en vivo (incluido)
UPDATE categories SET price = NULL    WHERE id = '22222222-0000-0000-0000-000000000007'; -- Mesa de mezclas (incluido)
UPDATE categories SET price = 700.00  WHERE id = '22222222-0000-0000-0000-000000000008'; -- Booth branding
UPDATE categories SET price = 2200.00 WHERE id = '22222222-0000-0000-0000-000000000009'; -- Bocinas y subwoofers
UPDATE categories SET price = 350.00  WHERE id = '22222222-0000-0000-0000-00000000000a'; -- Micrófonos

INSERT INTO modules (id, name, slug, icon, display_order, is_active) VALUES
    ('11111111-0000-0000-0000-000000000005', 'Personajes y Shows', 'personajes-y-shows', 'sparkles', 4, TRUE);

INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-00000000000b', '11111111-0000-0000-0000-000000000005', 'Botargas',
        'botargas', 'Personajes infantiles y de caricatura para animar la fiesta — por botarga.', 950.00, 0, TRUE),
    ('22222222-0000-0000-0000-00000000000c', '11111111-0000-0000-0000-000000000005', 'Robot de luces',
        'robot-de-luces', 'Robot LED de gran altura que baila e interactúa con los invitados.', 1900.00, 1, TRUE),
    ('22222222-0000-0000-0000-00000000000d', '11111111-0000-0000-0000-000000000005', 'Pajara Peggy',
        'pajara-peggy', 'El clásico personaje de avestruz que hace bailar a grandes y chicos.', 950.00, 2, TRUE);
