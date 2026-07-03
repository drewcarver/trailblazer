{
  description = "Dotnet dev environment with Home Manager";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

    flake-utils.url = "github:numtide/flake-utils";

    home-manager = {
      url = "github:nix-community/home-manager";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs = { self, nixpkgs, flake-utils, home-manager }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
      in
      {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
	    direnv
            dotnet-sdk_10
	    fsautocomplete
            neovim
            tmux
            git
            curl
            jq
            ripgrep
            fd
          ];
        };
      }
    )
    // {
      homeConfigurations.dev = {
        default = home-manager.lib.homeManagerConfiguration {
          pkgs = import nixpkgs { system = "x86_64-linux"; };

          modules = [
            ./home/home.nix
          ];
        };
      };
    };
}
