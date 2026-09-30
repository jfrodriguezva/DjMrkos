# Configurar correo del dominio con Zoho Mail

Manual para dejar `hola@tudominio.com` (o el correo que elijas) funcionando con Zoho Mail. Esto es independiente del despliegue del sitio ([deploy/README.md](README.md)) — el registro **A** que apunta el dominio al VPS y los registros **MX** de correo conviven sin problema en el mismo dominio.

> Los valores exactos de MX/SPF/DKIM que Zoho te pide **pueden variar** según tu región de cuenta (`zoho.com`, `zoho.eu`, `zoho.in`, etc.) y según cambie su producto. Trata los valores de este manual como referencia — siempre copia los que la propia consola de Zoho te muestre en el paso de verificación, no los transcribas de memoria.

## 0. Antes de empezar

- Acceso de administrador a la zona DNS del dominio (donde sea que esté — Zoho Domains, tu registrador, **Cloudflare**, etc.).
- Una cuenta de Zoho Mail (plan gratuito o de pago) en [zoho.com/mail](https://www.zoho.com/mail/) — si no tienes una, créala primero con "Add your existing domain", no con un correo genérico.
- Si el dominio **ya recibe correo con otro proveedor** (Gmail Workspace, Outlook, el hosting anterior, etc.), decide antes si vas a migrar por completo o no — cambiar los MX corta la entrega del correo del proveedor viejo casi de inmediato.

## 0.1 Si tu DNS vive en Cloudflare (dash.cloudflare.com)

Todo este manual dice "agrégalo en tu DNS" — en tu caso eso significa **dash.cloudflare.com**, no la consola de Zoho ni tu registrador. Zoho nunca gestiona DNS por sí mismo salvo que hayas comprado el dominio con Zoho Domains; solo te dice qué registros poner, y tú los agregas donde esté la zona DNS real.

Cómo agregar cualquiera de los registros de las secciones 1 a 5 (verificación, MX, SPF, DKIM, DMARC) en Cloudflare:

1. Entra a [dash.cloudflare.com](https://dash.cloudflare.com) → selecciona tu dominio (la "zona").
2. Menú lateral → **DNS** → **Records** → **Add record**.
3. Llena:
   - **Type**: el que Zoho te indique (CNAME, TXT o MX).
   - **Name**: la parte que Zoho te da (a veces es `@` para la raíz, a veces algo como `zmail._domainkey` o el código de verificación `zb123456`).
   - **Target / Content**: el valor exacto que Zoho te muestra.
   - **TTL**: Auto está bien.
4. **Proxy status — el punto crítico**: para **cada registro CNAME o TXT relacionado con Zoho** (verificación de dominio, DKIM), el ícono de la nube debe quedar **gris ("DNS only")**, nunca naranja ("Proxied"). Si lo dejas naranja, Cloudflare responde con su propia IP/proxy en lugar del valor real de Zoho, y la verificación de dominio o el DKIM van a fallar sin un mensaje de error claro. Los registros **MX no muestran esa opción** (Cloudflare nunca proxea correo), así que ahí no hay nada que ajustar.
5. Guarda, espera 1-2 minutos y regresa a Zoho Mail Admin Console a dar clic en **Verify**.

### El registro A de tu sitio (el del despliegue en el VPS) también debe quedar en gris

El [despliegue de este proyecto](README.md) usa Caddy para emitir su propio certificado HTTPS automáticamente (Let's Encrypt) contra la IP real del VPS. Si activas el proxy naranja de Cloudflare sobre el registro **A** de `tudominio.com` (y `www` si lo usas), Cloudflare se pone en medio y Caddy deja de poder validar el certificado por sí solo — necesitarías configurar Caddy con el plugin de DNS de Cloudflare (API token) para resolverlo, algo que **no** está armado en este proyecto todavía.

Mientras no hagas ese trabajo extra: deja el registro A también en **"DNS only" (nube gris)**. Cloudflare sigue sirviendo como tu panel de DNS — solo no está "proxeando" el tráfico — y todo funciona exactamente como si el DNS estuviera en cualquier otro proveedor.

## 1. Agrega el dominio en Zoho Mail

1. Entra a [Zoho Mail Admin Console](https://mailadmin.zoho.com).
2. **Domains** → **Add Domain** → escribe tu dominio (`tudominio.com`, sin `www`).
3. Elige el método de verificación. El más simple si ya vas a tocar DNS de todos modos es **CNAME**:
   - Zoho te da un registro tipo `Host: zb_______` → `Value: zmverify.zoho.com` (los valores reales aparecen en tu consola).
   - Agrégalo en tu proveedor de DNS.
4. Espera unos minutos a que propague y da clic en **Verify** dentro de Zoho. Si falla, espera más — la propagación de DNS puede tardar hasta un par de horas.

## 2. Registros MX (para que el correo llegue)

En la misma pantalla de configuración del dominio, Zoho te muestra los MX exactos a agregar. Los valores típicos para cuentas en la región global (`zoho.com`) son:

| Prioridad | Valor |
|---|---|
| 10 | `mx.zoho.com` |
| 20 | `mx2.zoho.com` |
| 50 | `mx3.zoho.com` |

Pasos:

1. En tu DNS, **borra cualquier registro MX existente** de un proveedor anterior para ese dominio (si lo hay) — dos proveedores de correo compitiendo por los mismos MX rompe la entrega.
2. Agrega los tres registros MX de la tabla (o los que tu consola te muestre — pueden diferir si tu cuenta es `zoho.eu`/`zoho.in`).
3. TTL: el default (1 hora / 3600 está bien).

## 3. SPF (que tu correo no se marque como spam)

Agrega un registro **TXT** en la raíz del dominio (`@`):

```
v=spf1 include:zoho.com ~all
```

Si el dominio ya tiene un TXT de SPF de otro servicio (por ejemplo, uno que dejó un proveedor anterior), **no agregues dos registros SPF** — se combinan en una sola línea con varios `include:`, por ejemplo:

```
v=spf1 include:zoho.com include:_spf.otroservicio.com ~all
```

Tener dos registros TXT de SPF separados es inválido y la mayoría de receptores de correo lo ignoran o lo rechazan.

## 4. DKIM (firma criptográfica del correo)

1. En Zoho Mail Admin Console → **Domains** → tu dominio → **DKIM**.
2. Genera una selección DKIM (Zoho te propone un nombre, ej. `zmail`).
3. Te da un registro **CNAME** o **TXT** tipo:
   - Host: `zmail._domainkey`
   - Valor: una cadena larga que empieza con `v=DKIM1; k=rsa; p=...`
4. Agrégalo en tu DNS y vuelve a la consola de Zoho a dar **Verify**.

## 5. DMARC (opcional, recomendado)

Agrega un TXT en `_dmarc.tudominio.com`:

```
v=DMARC1; p=none; rua=mailto:dmarc-reports@tudominio.com
```

Empieza con `p=none` (solo reporta, no rechaza nada) mientras confirmas que todo el correo legítimo pasa SPF/DKIM. Más adelante puedes subirlo a `p=quarantine` o `p=reject` para mayor protección contra spoofing.

## 6. Crea las cuentas de correo

### 6.1 Un usuario a la vez

1. [Zoho Mail Admin Console](https://mailadmin.zoho.com) → **Users** → **Add User**.
2. Llena:
   - **Email Address**: la parte antes de la `@` (ej. `hola`, queda `hola@tudominio.com`).
   - **Display Name**, **Password** (o deja que Zoho le mande un link para que la persona la ponga ella misma, si activas esa opción).
   - **Role**: usuario normal, a menos que quieras darle permisos de administrador del correo.
3. Guarda. La cuenta queda activa de inmediato — puedes entrar en [mail.zoho.com](https://mail.zoho.com) con ese usuario y contraseña.

Repite por cada cuenta que necesites (`ventas@tudominio.com`, `contacto@tudominio.com`, etc.).

### 6.2 Varias cuentas de un jalón (bulk)

Si necesitas crear muchas de una vez: **Users** → **Bulk Import Users** → descarga la plantilla CSV que te da Zoho, llena una fila por persona (email, nombre, contraseña) y súbela. Útil si vas a dar de alta a todo un equipo el mismo día.

### 6.3 Límite de cuentas según tu plan

Zoho Mail cambia sus planes con el tiempo, así que **verifica el límite actual en tu propia consola** (Admin Console → Billing/Plan Details) antes de asumir cuántas cuentas puedes crear gratis — el plan gratuito histórico ha rondado 5 usuarios con un tope de almacenamiento por buzón; los planes de pago (Mail Lite, Mail Premium) suben ese límite y agregan IMAP/POP completo, más espacio, etc.

### 6.4 Alias vs. cuenta nueva vs. grupo — cuál usar

No todo necesita ser una cuenta nueva con su propia contraseña:

- **Alias**: una dirección extra que cae en la bandeja de una cuenta que ya existe (ej. `info@tudominio.com` como alias de `hola@tudominio.com` — mismo buzón, misma contraseña, dos direcciones). Se configura en **Users** → selecciona el usuario → **Email Aliases** → **Add Alias**. No consume otro cupo de usuario.
- **Cuenta nueva**: cuando esa persona necesita su propio login y bandeja separada (ej. cada empleado con su nombre).
- **Grupo (distribution list)**: una dirección que **reenvía** a varias personas a la vez (ej. `contacto@tudominio.com` → llega una copia a `hola@` y a `ventas@`). Se crea en **Groups** → **Add Group**, y ahí agregas los miembros. No es un buzón en sí — no se puede iniciar sesión con la dirección del grupo.

Para DJ MrKos, lo más probable es que solo necesites **una cuenta** (`hola@tudominio.com`, el placeholder que está en `frontend/src/lib/contact.ts`) y quizás un alias si más adelante quieres que `contacto@` o `info@` lleguen al mismo lugar.

### 6.5 Acceso desde el celular / un cliente de correo

Además de la webmail (mail.zoho.com) o la app oficial **Zoho Mail** (iOS/Android), cada cuenta puede configurarse en Gmail app, Outlook, Apple Mail, etc. con estos datos (Admin Console → Users → el usuario → **Mail Settings** te confirma los tuyos exactos, pero típicamente):

| | Servidor | Puerto | Seguridad |
|---|---|---|---|
| IMAP (entrante) | `imap.zoho.com` | 993 | SSL |
| POP (entrante, alternativa) | `pop.zoho.com` | 995 | SSL |
| SMTP (saliente) | `smtp.zoho.com` | 465 | SSL |

Usuario: el correo completo (`hola@tudominio.com`). Contraseña: la de esa cuenta (o una "App Password" específica si tienes verificación en dos pasos activada — se genera en **Zoho Account** → **Security** → **App Passwords**).

## 7. Prueba de extremo a extremo

1. Desde otra cuenta (Gmail, por ejemplo), manda un correo a `hola@tudominio.com` — debe llegar en segundos a minutos.
2. Desde `hola@tudominio.com`, responde — revisa en el Gmail que **no** haya caído en spam y que el remitente se vea correcto (no "vía otro-dominio.com").
3. Herramientas útiles para verificar que SPF/DKIM/DMARC quedaron bien:
   - [mail-tester.com](https://www.mail-tester.com) — manda un correo a la dirección que te da y te califica la configuración.
   - `nslookup -type=MX tudominio.com` y `nslookup -type=TXT tudominio.com` para confirmar que los registros ya propagaron.

## 8. Actualiza el placeholder en el código

Una vez que el correo funcione de verdad, actualiza `frontend/src/lib/contact.ts` — ahí sigue `hola@djmrkos.com` como placeholder. Si también vas a usar WhatsApp/teléfono reales, actualiza `whatsappNumber`, `phoneDisplay` y `phoneHref` en el mismo archivo.

## Problemas comunes

- **"Domain not verified" después de agregar el CNAME**: espera más tiempo de propagación, o revisa que no dejaste espacios/puntos extra al copiar el valor.
- **El correo no llega**: casi siempre son los MX — confirma con `nslookup -type=MX tudominio.com` que solo aparecen los de Zoho, sin ninguno de un proveedor anterior mezclado.
- **Cae en spam**: revisa que SPF y DKIM estén verdes ("Verified") en la consola de Zoho, y usa mail-tester.com para un diagnóstico puntual.
- **Tenías Google Workspace/Outlook antes**: mientras el DNS viejo siga cacheado en algunos servidores (hasta 48h), puede que algo de correo siga intentando llegar al proveedor anterior — es temporal.
