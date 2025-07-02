# Security Policy

## Supported Versions

We only provide security updates and support for the following versions of this web forum backend:

| Version | Supported          |
| ------- | ------------------ |
| 5.1.x   | :white_check_mark: |
| 5.0.x   | :x:                |
| 4.0.x   | :white_check_mark: |
| < 4.0   | :x:                |

## Reporting a Vulnerability

If you believe you have found a security vulnerability in this ASP.NET web forum application, **please help us keep the community safe** by following these guidelines:

1. **Do not publicly disclose** the vulnerability until we have had a chance to review and address it.
2. **Contact the maintainers** by opening a confidential issue in this repository, or emailing us at: **[baloyi.jabu@gmail.com]**.
3. When reporting, include as much relevant information as possible:
    - A description of the vulnerability (e.g., authentication bypass, privilege escalation, SQL injection, etc.)
    - Steps to reproduce the issue, including example requests if possible
    - Any affected endpoints, API routes, or user roles (regular user, moderator, etc.)
    - The version number where you discovered the vulnerability
4. We will acknowledge your report **within 2 business days**.
5. We will provide status updates at least once per week until the issue is resolved.
6. After a fix is released, we will coordinate with you on public disclosure and crediting if desired.
7. If the report is not accepted as a security vulnerability (e.g., working as intended, or already reported), we will provide an explanation.

**Important Security Areas in This Application:**
- **User authentication:** Password security, session management, and protection against brute-force attacks.
- **Authorization:** Users should not be able to perform actions reserved for moderators or like their own posts.
- **Data validation:** Inputs should be validated to prevent SQL injection and other attacks.
- **API access:** Only authenticated users can post, comment, or like. Anonymous users can only view posts.
- **Rate limiting:** Protection against automated abuse or brute-force attacks (if implemented).
- **Sensitive data:** User passwords are never stored in plain text and are hashed securely.

We welcome responsible disclosure and thank you for helping to secure this project for all users!
