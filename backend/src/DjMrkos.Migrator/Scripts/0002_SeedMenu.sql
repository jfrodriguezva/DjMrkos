-- Datos semilla: el catálogo de ejemplo del brief de marca (Luces, Música, Cabina, Sonido).
-- Sirve para levantar el portal con contenido real desde el primer `dotnet run`.

INSERT INTO modules (id, name, slug, icon, display_order, is_active) VALUES
    ('11111111-0000-0000-0000-000000000001', 'Luces',  'luces',  'lightbulb', 0, 1),
    ('11111111-0000-0000-0000-000000000002', 'Música', 'musica', 'disc',      1, 1),
    ('11111111-0000-0000-0000-000000000003', 'Cabina', 'cabina', 'mixer',     2, 1),
    ('11111111-0000-0000-0000-000000000004', 'Sonido', 'sonido', 'speaker',   3, 1);

INSERT INTO categories (id, module_id, name, slug, description, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-000000000001', '11111111-0000-0000-0000-000000000001', 'Torre de luces', 'torre-de-luces', 'Estructuras verticales con múltiples cabezas móviles.', 0, 1),
    ('22222222-0000-0000-0000-000000000002', '11111111-0000-0000-0000-000000000001', 'Reflectores',    'reflectores',    'Iluminación de ambiente para pista y decoración.', 1, 1),
    ('22222222-0000-0000-0000-000000000003', '11111111-0000-0000-0000-000000000001', 'LED RGB',        'led-rgb',        'Paneles y tiras de color programable.', 2, 1),
    ('22222222-0000-0000-0000-000000000004', '11111111-0000-0000-0000-000000000001', 'Láser',          'laser',          'Efectos de láser sincronizados con la música.', 3, 1),

    ('22222222-0000-0000-0000-000000000005', '11111111-0000-0000-0000-000000000002', 'Géneros',         'generos',         'Pop, regional, electrónica, salsa y más — mezclados en vivo.', 0, 1),
    ('22222222-0000-0000-0000-000000000006', '11111111-0000-0000-0000-000000000002', 'Mezclas en vivo', 'mezclas-en-vivo', 'Transiciones y mashups preparados para cada momento del evento.', 1, 1),

    ('22222222-0000-0000-0000-000000000007', '11111111-0000-0000-0000-000000000003', 'Mesa de mezclas', 'mesa-de-mezclas', 'Controladores profesionales de última generación.', 0, 1),
    ('22222222-0000-0000-0000-000000000008', '11111111-0000-0000-0000-000000000003', 'Booth branding',  'booth-branding',  'Cabina personalizada con la imagen de tu evento.', 1, 1),

    ('22222222-0000-0000-0000-000000000009', '11111111-0000-0000-0000-000000000004', 'Bocinas y subwoofers', 'bocinas-y-subwoofers', 'Cobertura de sonido para pista y salón completo.', 0, 1),
    ('22222222-0000-0000-0000-00000000000a', '11111111-0000-0000-0000-000000000004', 'Micrófonos',           'microfonos',           'Inalámbricos para maestro de ceremonias y discursos.', 1, 1);
