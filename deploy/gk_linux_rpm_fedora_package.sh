#!/bin/sh

APP_VER="$1"
DIR="$( cd "$( dirname "$0" )" && pwd )"

echo "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"
echo "Install the build dependencies"
echo "dnf in /usr/bin/rpmdev-setuptree dotnet-sdk-6.0"
echo "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"

rm -rf ~/rpmbuild/
rpmdev-setuptree

tar -zcf ~/rpmbuild/SOURCES/gedkeeper-$APP_VER.tar.gz \
    	./gk_run.sh \
	./gedkeeper.png \
	./gedkeeper.desktop \
	./application-x-gedkeeper.xml \
	./gedkeeper.appdata.xml \
	../bin \
	../LICENSE \
	../locales/ \
	../plugins/ \
	../scripts/ \
	../samples/ \
	../themes/ \
	../externals/resources.yaml

cp "$DIR/rpm/gedkeeper.spec" ~/rpmbuild/SPECS/gedkeeper.spec

pushd ~/rpmbuild/SPECS/
# build from binary
rpmbuild -bb gedkeeper.spec
popd

cd "$DIR"

