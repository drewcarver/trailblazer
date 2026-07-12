{
  description = "HikePlanner dev environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
      in
      {
        devShells.default = pkgs.mkShell {
          buildInputs = [
            (pkgs.python3.withPackages (ps: with ps; [
              pandas
            ]))
          ];

          packages = with pkgs; [
            dotnet-sdk_10
            python3
            fsautocomplete
            fantomas
            git
            curl
            jq
            ripgrep
            fd
          ];

          shellHook = ''
            echo "HikePlanner dev shell loaded"
          '';
        };
      }
    );
}
