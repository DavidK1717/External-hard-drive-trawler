-- Database: `filesearch`
--
CREATE DATABASE IF NOT EXISTS `filesearch`;
USE `filesearch`;

DELIMITER $$
--
-- Procedures
--
DROP PROCEDURE IF EXISTS `add_uae`$$
CREATE PROCEDURE `add_uae` (IN `p_vol` VARCHAR(10), IN `p_path` VARCHAR(1000))  MODIFIES SQL DATA
insert into uae (volume_label, dir_path)
values (p_vol, p_path)$$

DROP PROCEDURE IF EXISTS `create_drive_record`$$
CREATE PROCEDURE `create_drive_record` (IN `p_vol` VARCHAR(10), IN `p_total_cap` BIGINT, IN `p_available_space` BIGINT, IN `P_filesystem` VARCHAR(10))  MODIFIES SQL DATA
insert into drive(volume_label, total_capacity, available_space, file_system) values (p_vol, p_total_cap, p_available_space, p_filesystem)$$

DROP PROCEDURE IF EXISTS `create_file_record`$$
CREATE PROCEDURE `create_file_record` (IN `p_volume` VARCHAR(10), IN `p_path` VARCHAR(1000), IN `p_file_name` VARCHAR(1000), IN `p_file_ext` VARCHAR(10), IN `p_size` BIGINT)  MODIFIES SQL DATA
insert into file(volume_label, filepath, filename, file_ex, size) values (p_volume, p_path, p_file_name, p_file_ext, p_size)$$

DROP PROCEDURE IF EXISTS `drive_check`$$
CREATE PROCEDURE `drive_check` (IN `p_vol` VARCHAR(10))  READS SQL DATA
select count(*) from drive where volume_label = P_vol$$

DROP PROCEDURE IF EXISTS `update_counts`$$
CREATE PROCEDURE `update_counts` (IN `p_vol` VARCHAR(10), IN `p_folder_count` BIGINT, IN `p_file_count` BIGINT)  MODIFIES SQL DATA
update drive
set folder_count=p_folder_count,
file_count=p_file_count,
end_time=NOW()
where volume_label=p_vol$$

DROP PROCEDURE IF EXISTS `update_drive_record`$$
CREATE PROCEDURE `update_drive_record` (IN `p_vol` VARCHAR(10), IN `p_total_cap` BIGINT, IN `p_available_space` BIGINT, IN `p_filesystem` VARCHAR(10))  MODIFIES SQL DATA
BEGIN
update drive
set
total_capacity=p_total_cap, available_space = p_available_space, file_system = p_filesystem,
start_time=NOW()
where volume_label = p_vol;
END$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `drive`
--

DROP TABLE IF EXISTS `drive`;
CREATE TABLE `drive` (
  `volume_label` varchar(10) NOT NULL,
  `total_capacity` bigint(20) NOT NULL,
  `available_space` bigint(20) NOT NULL,
  `file_system` varchar(20) NOT NULL,
  `folder_count` bigint(20) DEFAULT NULL,
  `file_count` bigint(20) DEFAULT NULL,
  `start_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `end_time` datetime DEFAULT NULL
);

-- --------------------------------------------------------

--
-- Table structure for table `file`
--

DROP TABLE IF EXISTS `file`;
CREATE TABLE `file` (
  `file_id` int(11) NOT NULL,
  `volume_label` varchar(10) NOT NULL,
  `file_ex` varchar(10) DEFAULT NULL,
  `filename` varchar(1000) NOT NULL,
  `filepath` varchar(1000) NOT NULL,
  `size` bigint(20) NOT NULL
);

-- --------------------------------------------------------

--
-- Table structure for table `uae`
--

DROP TABLE IF EXISTS `uae`;
CREATE TABLE `uae` (
  `uae_id` int(11) NOT NULL,
  `volume_label` varchar(10) NOT NULL,
  `dir_path` varchar(1000) NOT NULL,
  `ex_time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `drive`
--
ALTER TABLE `drive`
  ADD PRIMARY KEY (`volume_label`);

--
-- Indexes for table `file`
--
ALTER TABLE `file`
  ADD PRIMARY KEY (`file_id`);

--
-- Indexes for table `uae`
--
ALTER TABLE `uae`
  ADD PRIMARY KEY (`uae_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `file`
--
ALTER TABLE `file`
  MODIFY `file_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `uae`
--
ALTER TABLE `uae`
  MODIFY `uae_id` int(11) NOT NULL AUTO_INCREMENT;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
