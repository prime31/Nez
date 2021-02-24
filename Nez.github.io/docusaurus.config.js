module.exports = {
  title: 'Nez',
  tagline: 'Nez is a free 2D focused framework that works with MonoGame and FNA ',
  url: 'https://prime31.github.io/',
  baseUrl: '/Nez/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'prime31', // Usually your GitHub org/user name.
  projectName: 'Nez', // Usually your repo name.
  themeConfig: {
    navbar: {
      logo: {
        alt: 'Nez Logo',
        src: 'img/logo.svg',
      },
      items: [
        {
          to: 'docs/',
          activeBasePath: 'docs',
          label: 'Docs',
          position: 'left',
        },
        {to: 'blog', label: 'Blog', position: 'left'},
        {
          href: 'https://github.com/prime31/Nez',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Installation',
              to: 'docs/',
            },
            {
              label: 'Features',
              to: 'docs/features/core',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'Discord',
              href: 'https://discord.gg/fuNrR4jF5Z',
            },
            {
              label: 'Twitter',
              href: 'https://twitter.com/prime_31',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'Blog',
              to: 'blog',
            },
            {
              label: 'GitHub',
              href: 'https://github.com/prime31/Nez',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Prime31, Inc. Built with Docusaurus.`,
    },
    prism: {
      additionalLanguages: ['csharp'],
      theme: require('prism-react-renderer/themes/github'),
      darkTheme: require('prism-react-renderer/themes/vsDark')
    }
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl:
          'https://github.com/prime31/Nez/edit/master/Nez.github.io/',
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          editUrl:
            'https://github.com/prime31/Nez/edit/master/Nez.github.io/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
};
