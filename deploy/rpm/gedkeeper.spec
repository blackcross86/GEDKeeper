%define	        rname GEDKeeper
%define	        summary GEDKeeper - program for work with personal genealogical database
%define		__brp_strip %{nil}

Name:		gedkeeper
Version:	3.2.1
Release:	1%{dist}
Summary:	%{summary}
License:	GPLv3
Group:		Applications/Editors
Url:		https://github.com/serg-norseman/gedkeeper
Source:         %{name}-%{version}.tar.gz
ExclusiveArch:	x86_64

Requires:	lua
Requires:	mono-core
Requires:	sqlite

AutoReq:	no
AutoReqProv:	no

%description
%{summary}.

%files
%license LICENSE
%{_bindir}/gk_run.sh
%{_libdir}/%{name}
%{_datadir}/mime/*.xml
%{_datadir}/applications/%{name}.desktop
%{_datadir}/pixmaps/%{name}.png

%prep
%setup -qc
find . -type f -iname "*.dll" -exec chmod -x {} \;
find ./locales -type f -exec chmod -x '{}' \;
find ./plugins -type f -exec chmod -x '{}' \;
find ./scripts -type f -exec chmod -x '{}' \;
find ./samples -type f -exec chmod -x '{}' \;
find . -name "*.so" -exec strip '{}' \;

%install
install -Dm 0755 gk_run.sh %{buildroot}%{_bindir}/gk_run.sh
install -d 0755 %{buildroot}%{_libdir}/%{name}
install -Dm 0644 application-x-%{name}.xml %{buildroot}%{_datadir}/mime/application-x-%{name}.xml
install -Dm 0644 %{name}.desktop %{buildroot}%{_datadir}/applications/%{name}.desktop
install -Dm 0644 %{name}.png %{buildroot}%{_datadir}/pixmaps/%{name}.png
cp -r bin \
	locales \
	plugins \
	samples \
	scripts %{buildroot}%{_libdir}/%{name}

## E: zero-length
rm -rf %{buildroot}%{_libdir}/%{name}/scripts/readme.txt

%changelog
* Apr 28 2023 GEDKeeper - 3.2.1
- New upstream release

