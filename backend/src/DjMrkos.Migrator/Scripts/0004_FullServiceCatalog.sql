-- Catálogo real de DJ MrKos, tal como lo cotiza el negocio. Reemplaza por completo el
-- catálogo de ejemplo sembrado en 0002/0003 — nada más referencia modules/categories por
-- fuera de estas dos tablas (song_requests y testimonials cuelgan de events, no del catálogo),
-- así que es seguro vaciarlas y volver a poblarlas en una sola migración.

DELETE FROM categories;
DELETE FROM modules;

-- ============================================================
-- Módulos
-- ============================================================
INSERT INTO modules (id, name, slug, icon, display_order, is_active) VALUES
    ('11111111-0000-0000-0000-000000000001', 'Personal y Staff',        'personal-y-staff',        'users',    0, 1),
    ('11111111-0000-0000-0000-000000000002', 'Cabina',                  'cabina',                   'mixer',    1, 1),
    ('11111111-0000-0000-0000-000000000003', 'Iluminación',             'iluminacion',              'lightbulb',2, 1),
    ('11111111-0000-0000-0000-000000000004', 'Efectos Especiales',      'efectos-especiales',       'sparkles', 3, 1),
    ('11111111-0000-0000-0000-000000000005', 'Pantallas y Proyección',  'pantallas-y-proyeccion',   'monitor',  4, 1),
    ('11111111-0000-0000-0000-000000000006', 'Audio Profesional',       'audio-profesional',        'speaker',  5, 1),
    ('11111111-0000-0000-0000-000000000007', 'Personajes y Shows',      'personajes-y-shows',       'mask',     6, 1);

-- ============================================================
-- Personal y Staff
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-000000000001', '11111111-0000-0000-0000-000000000001', 'Servicio DJ',
        'servicio-dj', 'Mezcla profesional en vivo durante todo tu evento.', 2500.00, 0, 1),
    ('22222222-0000-0000-0000-000000000002', '11111111-0000-0000-0000-000000000001', 'Animador',
        'animador', 'Dirige la dinámica y mantiene la pista activa toda la noche.', 2000.00, 1, 1),
    ('22222222-0000-0000-0000-000000000003', '11111111-0000-0000-0000-000000000001', 'Maestro de ceremonias',
        'maestro-de-ceremonias', 'Conduce el protocolo y los momentos especiales del evento.', 2000.00, 2, 1),
    ('22222222-0000-0000-0000-000000000004', '11111111-0000-0000-0000-000000000001', 'Ingeniero de sonido',
        'ingeniero-de-sonido', 'Operación y afinación de audio en tiempo real.', 2500.00, 3, 1),
    ('22222222-0000-0000-0000-000000000005', '11111111-0000-0000-0000-000000000001', 'Ingeniero en iluminación',
        'ingeniero-en-iluminacion', 'Programación y operación de todo el equipo de luces.', 1500.00, 4, 1),
    ('22222222-0000-0000-0000-000000000006', '11111111-0000-0000-0000-000000000001', 'Staff',
        'staff', 'Personal de apoyo para montaje, desmontaje y logística.', 1500.00, 5, 1),
    ('22222222-0000-0000-0000-000000000007', '11111111-0000-0000-0000-000000000001', 'Renta de equipo para terceros',
        'renta-de-equipo-para-terceros', 'Para cantantes, comediantes, grupos o bandas que se presenten en tu evento.', 2000.00, 6, 1);

-- ============================================================
-- Cabina
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-000000000008', '11111111-0000-0000-0000-000000000002', 'Cabina DJ',
        'cabina-dj', 'Controlador profesional Pioneer o Numark — varios modelos disponibles.', 2000.00, 0, 1),
    ('22222222-0000-0000-0000-000000000009', '11111111-0000-0000-0000-000000000002', 'Cabina vintage',
        'cabina-vintage', 'Diseño retro, ideal para bodas y eventos con temática clásica.', 500.00, 1, 1),
    ('22222222-0000-0000-0000-00000000000a', '11111111-0000-0000-0000-000000000002', 'Cabina LED',
        'cabina-led', 'Cabina iluminada que se adapta al color de tu decoración.', 1500.00, 2, 1),
    ('22222222-0000-0000-0000-00000000000b', '11111111-0000-0000-0000-000000000002', 'Cabina 360 Photo Booth',
        'cabina-360-photo-booth', '2 horas de servicio, videos ilimitados para tus invitados.', 4500.00, 3, 1);

-- ============================================================
-- Iluminación
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-00000000000c', '11111111-0000-0000-0000-000000000003', 'Iluminación Beam',
        'iluminacion-beam', 'Rayos de luz de largo alcance para pista y escenario — precio por unidad.', 500.00, 0, 1),
    ('22222222-0000-0000-0000-00000000000d', '11111111-0000-0000-0000-000000000003', 'Iluminación Wash',
        'iluminacion-wash', 'Baño de luz de color para ambientar todo el salón — precio por unidad.', 500.00, 1, 1),
    ('22222222-0000-0000-0000-00000000000e', '11111111-0000-0000-0000-000000000003', 'Iluminación Láser',
        'iluminacion-laser', 'Efectos de láser sincronizados con la música — precio por unidad.', 500.00, 2, 1),
    ('22222222-0000-0000-0000-00000000000f', '11111111-0000-0000-0000-000000000003', 'Iluminación arquitectónica',
        'iluminacion-arquitectonica', 'Ilumina fachadas y estructuras, interior o exterior — precio por unidad.', 500.00, 3, 1),
    ('22222222-0000-0000-0000-000000000010', '11111111-0000-0000-0000-000000000003', 'Robot LED piso',
        'robot-led-piso', 'Robot LED de gran altura que interactúa con los invitados.', 2500.00, 4, 1),
    ('22222222-0000-0000-0000-000000000011', '11111111-0000-0000-0000-000000000003', 'Robot LED con zancos',
        'robot-led-con-zancos', 'Robot LED operado por un performer en zancos.', 3000.00, 5, 1),
    ('22222222-0000-0000-0000-000000000012', '11111111-0000-0000-0000-000000000003', 'Pista LED',
        'pista-led', 'Piso LED programable — se cotiza por módulo.', 850.00, 6, 1);

-- ============================================================
-- Efectos Especiales
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-000000000013', '11111111-0000-0000-0000-000000000004', 'Máquina de humo',
        'maquina-de-humo', 'Efecto de humo para resaltar la iluminación.', 1000.00, 0, 1),
    ('22222222-0000-0000-0000-000000000014', '11111111-0000-0000-0000-000000000004', 'Chisperos controlados',
        'chisperos-controlados', 'Chispas frías operadas por máquina profesional — permitidas dentro de salones, no es pirotecnia. Precio por unidad.', 1500.00, 1, 1),
    ('22222222-0000-0000-0000-000000000015', '11111111-0000-0000-0000-000000000004', 'Bazuca de confeti',
        'bazuca-de-confeti', '3 disparos de confeti para el momento culminante.', 1500.00, 2, 1),
    ('22222222-0000-0000-0000-000000000016', '11111111-0000-0000-0000-000000000004', 'Máquina de niebla baja',
        'maquina-de-niebla-baja', 'Un disparo — ideal para el baile familiar o el vals.', 2500.00, 3, 1),
    ('22222222-0000-0000-0000-000000000017', '11111111-0000-0000-0000-000000000004', 'Máquina de niebla densa con hielo seco',
        'maquina-de-niebla-densa-con-hielo-seco', 'Un disparo — efecto denso a ras de piso para el vals.', 3500.00, 4, 1);

-- ============================================================
-- Pantallas y Proyección
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-000000000018', '11111111-0000-0000-0000-000000000005', 'Pantallas',
        'pantallas', 'Para transmitir en vivo o mostrar contenido audiovisual — precio por unidad.', 1500.00, 0, 1),
    ('22222222-0000-0000-0000-000000000019', '11111111-0000-0000-0000-000000000005', 'Proyectores',
        'proyectores', 'Incluye tela de proyección — precio por unidad.', 750.00, 1, 1),
    ('22222222-0000-0000-0000-00000000001a', '11111111-0000-0000-0000-000000000005', 'Pantalla LED 3x2',
        'pantalla-led-3x2', 'Pantalla LED de alta resolución, interior o exterior.', 10000.00, 2, 1);

-- ============================================================
-- Audio Profesional
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-00000000001b', '11111111-0000-0000-0000-000000000006', 'Audio profesional 50 a 150 personas',
        'audio-50-a-150-personas', '5 horas de servicio continuo.', 3500.00, 0, 1),
    ('22222222-0000-0000-0000-00000000001c', '11111111-0000-0000-0000-000000000006', 'Audio profesional 150 a 300 personas',
        'audio-150-a-300-personas', '5 horas de servicio continuo.', 5500.00, 1, 1),
    ('22222222-0000-0000-0000-00000000001d', '11111111-0000-0000-0000-000000000006', 'Audio profesional 300 a 600 personas',
        'audio-300-a-600-personas', '5 horas de servicio continuo.', 7500.00, 2, 1),
    ('22222222-0000-0000-0000-00000000001e', '11111111-0000-0000-0000-000000000006', 'Audio profesional 600 a 1000 personas',
        'audio-600-a-1000-personas', '5 horas de servicio continuo.', 12500.00, 3, 1);

-- ============================================================
-- Personajes y Shows
-- ============================================================
INSERT INTO categories (id, module_id, name, slug, description, price, display_order, is_active) VALUES
    ('22222222-0000-0000-0000-00000000001f', '11111111-0000-0000-0000-000000000007', 'Pájara Peggy',
        'pajara-peggy', 'El clásico personaje que hace bailar a grandes y chicos.', 1500.00, 0, 1),
    ('22222222-0000-0000-0000-000000000020', '11111111-0000-0000-0000-000000000007', 'Cabezón Daddy Yankee',
        'cabezon-daddy-yankee', 'Personaje de cabezón gigante para fotos y baile.', 1500.00, 1, 1),
    ('22222222-0000-0000-0000-000000000021', '11111111-0000-0000-0000-000000000007', 'Cabezón Bad Bunny',
        'cabezon-bad-bunny', 'Personaje de cabezón gigante para fotos y baile.', 1500.00, 2, 1),
    ('22222222-0000-0000-0000-000000000022', '11111111-0000-0000-0000-000000000007', 'Cabezón Karol G',
        'cabezon-karol-g', 'Personaje de cabezón gigante para fotos y baile.', 1500.00, 3, 1),
    ('22222222-0000-0000-0000-000000000023', '11111111-0000-0000-0000-000000000007', 'Cabezón Luis Miguel',
        'cabezon-luis-miguel', 'Personaje de cabezón gigante para fotos y baile.', 1500.00, 4, 1),
    ('22222222-0000-0000-0000-000000000024', '11111111-0000-0000-0000-000000000007', 'Botarga de Simmi',
        'botarga-de-simmi', 'Personaje infantil para animar la fiesta.', 1500.00, 5, 1),
    ('22222222-0000-0000-0000-000000000025', '11111111-0000-0000-0000-000000000007', 'Botarga gorila gigante',
        'botarga-gorila-gigante', 'Botarga de gran formato — sorprende a todas las edades.', 3500.00, 6, 1),
    ('22222222-0000-0000-0000-000000000026', '11111111-0000-0000-0000-000000000007', 'Zanquero batucada',
        'zanquero-batucada', 'Performer en zancos con ritmo de batucada.', 2000.00, 7, 1),
    ('22222222-0000-0000-0000-000000000027', '11111111-0000-0000-0000-000000000007', 'Zanquero 70s',
        'zanquero-70s', 'Performer en zancos con temática retro de los años 70.', 2000.00, 8, 1),
    ('22222222-0000-0000-0000-000000000028', '11111111-0000-0000-0000-000000000007', 'Robocop',
        'robocop', 'Personaje robot interactivo para fotos y animación.', 3000.00, 9, 1),
    ('22222222-0000-0000-0000-000000000029', '11111111-0000-0000-0000-000000000007', 'Show mariachi pastel',
        'show-mariachi-pastel', 'Presentación de mariachi para el corte de pastel.', 2000.00, 10, 1),
    ('22222222-0000-0000-0000-00000000002a', '11111111-0000-0000-0000-000000000007', 'Show YMCA',
        'show-ymca', 'Show coreografiado del clásico YMCA.', 2000.00, 11, 1),
    ('22222222-0000-0000-0000-00000000002b', '11111111-0000-0000-0000-000000000007', 'Show norteños y banda',
        'show-nortenos-banda', 'Presentación en vivo de música norteña o banda.', 2000.00, 12, 1),
    ('22222222-0000-0000-0000-00000000002c', '11111111-0000-0000-0000-000000000007', 'Show luchadores y rin',
        'show-luchadores-y-rin', 'Máscaras de luchador, incluye ring.', 2000.00, 13, 1);
