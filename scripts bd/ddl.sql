SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema 5to_Domotica
-- -----------------------------------------------------
DROP SCHEMA IF EXISTS `5to_Domotica`;

-- -----------------------------------------------------
-- Schema 5to_Domotica
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `5to_Domotica` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `5to_Domotica`;

-- -----------------------------------------------------
-- Table `Usuario`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Usuario` (
  `idUsuario` INT NOT NULL AUTO_INCREMENT,
  `Nombre` VARCHAR(100) NOT NULL,
  `Correo` VARCHAR(100) NOT NULL,
  `Contrasenia` CHAR(64) NOT NULL,
  `Telefono` VARCHAR(20) NULL,
  PRIMARY KEY (`idUsuario`),
  UNIQUE INDEX `Correo_UNIQUE` (`Correo`)
) ENGINE=InnoDB;

-- -----------------------------------------------------
-- Table `Casa`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Casa` (
  `idCasa` INT NOT NULL AUTO_INCREMENT,
  `Direccion` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`idCasa`)
) ENGINE=InnoDB;

-- -----------------------------------------------------
-- Table `Electrodomestico`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Electrodomestico` (
  `idElectrodomestico` INT NOT NULL AUTO_INCREMENT,
  `idCasa` INT NOT NULL,
  `Nombre` VARCHAR(100) NOT NULL,
  `Tipo` VARCHAR(50) NOT NULL,
  `Ubicacion` VARCHAR(50) NOT NULL,
  `Encendido` BOOLEAN NOT NULL,
  `ConsumoPorHora` FLOAT NOT NULL,
  PRIMARY KEY (`idElectrodomestico`),
  UNIQUE INDEX `Ubicacion_UNIQUE` (`Ubicacion`),
  CONSTRAINT `fk_Electrodomestico_Casa`
    FOREIGN KEY (`idCasa`)
    REFERENCES `Casa` (`idCasa`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB;

-- -----------------------------------------------------
-- Table `Consumo`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `Consumo` (
  `idConsumo` INT NOT NULL AUTO_INCREMENT,
  `idElectrodomestico` INT NOT NULL,
  `inicio` DATETIME NOT NULL,
  `duracion` TIME NOT NULL,
  `consumoTotal` FLOAT NOT NULL,
  PRIMARY KEY (`idConsumo`),
  CONSTRAINT `fk_Consumo_Electrodomestico`
    FOREIGN KEY (`idElectrodomestico`)
    REFERENCES `Electrodomestico` (`idElectrodomestico`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB;

-- -----------------------------------------------------
-- Table `HistorialRegistro`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `HistorialRegistro` (
  `idElectrodomestico` INT NOT NULL,
  `fechaHoraRegistro` DATETIME NOT NULL,
  PRIMARY KEY (`idElectrodomestico`, `fechaHoraRegistro`),
  CONSTRAINT `fk_HistorialRegistro_Electrodomestico`
    FOREIGN KEY (`idElectrodomestico`)
    REFERENCES `Electrodomestico` (`idElectrodomestico`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB;

-- -----------------------------------------------------
-- Table `casaUsuario`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `casaUsuario` (
  `idUsuario` INT NOT NULL,
  `idCasa` INT NOT NULL,
  PRIMARY KEY (`idUsuario`, `idCasa`),
  CONSTRAINT `fk_casaUsuario_Usuario`
    FOREIGN KEY (`idUsuario`)
    REFERENCES `Usuario` (`idUsuario`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_casaUsuario_Casa`
    FOREIGN KEY (`idCasa`)
    REFERENCES `Casa` (`idCasa`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE=InnoDB;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
